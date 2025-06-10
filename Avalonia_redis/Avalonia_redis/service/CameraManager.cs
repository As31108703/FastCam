using Emgu.CV;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DirectShowLib;
using Avalonia_redis;
using System.Linq;
using Avalonia.Threading;

public class CameraManager
{
  private VideoCapture[] _captures;
  private List<CancellationTokenSource> _cancellationTokenSources;
  private DsDevice[] _systemCameras;
  private AppConfig _config;
  private UIManager _uiManager;
  private Redismanager _redisManager;
  private VideoCapture.API _api;
  string[] _displayChannels;
  DsDevice[] systemCameras = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);

  public CameraManager(AppConfig config, string apiName, DsDevice[] systemCameras, UIManager uiManager, Redismanager redisManager)
  {
    _config = config;
    _uiManager = uiManager;
    _redisManager = redisManager;
    _api = VideoCaptureApiHelper.GetVideoCaptureAPI(apiName);
    _systemCameras = systemCameras;
    _captures = new VideoCapture[systemCameras.Length];
    _cancellationTokenSources = new List<CancellationTokenSource>();
    _displayChannels = new[]
      {
        _config.DISPLAY_ONE,
        _config.DISPLAY_TWO,
        _config.DISPLAY_THREE,
        _config.DISPLAY_FOUR,
        _config.DISPLAY_FIVE,
        _config.DISPLAY_SIX,
        _config.DISPLAY_SEVEN
      };
  }

  public void InitializeCameras()
  {
    for (int i = 0; i < _systemCameras.Length && i < _config.MAX_CAM_COUNT; i++)
    {
      try
      {
        _captures[i] = new VideoCapture(i, _api);
        Log.Information($"攝像頭{i}的ID->{_systemCameras[i].DevicePath}");

        if (!_captures[i].IsOpened)
        {
          Log.Error($"攝像頭 {i} 初始化失敗，無法打開");
          continue;
        }

        InitializeCamera(_captures[i]);
      }
      catch (Exception ex)
      {
        Log.Error($"初始化攝像頭 {i} 時發生錯誤: {ex.Message}");
      }
    }
  }

  public void InitializeCamera(VideoCapture capture)
  {
    try
    {
      if (!capture.IsOpened)
      {
        Log.Error("攝像頭初始化失敗，無法打開");
        return;
      }

      int cameraIndex = Array.IndexOf(_captures, capture); // 獲取當前攝像頭的索引
      string cameraPath = systemCameras[cameraIndex].DevicePath; // 獲取攝影機名稱

      // 設置攝影機的畫面寬度和高度
      ConfigureCamera(capture);

      // 啟動攝影機捕獲
      Task.Run(async () =>
      {
        try
        {
          await StartCapture(cameraPath); // 使用攝影機名稱
        }
        catch (Exception ex)
        {
          Log.Error($"啟動攝像頭 {cameraPath} 時發生錯誤: {ex.Message}");
        }
      });

      Log.Information($"攝像頭{cameraPath}初始化成功");
    }
    catch (Exception ex)
    {
      Log.Error($"初始化攝像頭時發生錯誤: {ex.Message}");
    }
  }

  public void ConfigureCamera(VideoCapture capture)
  {
    int cameraIndex = Array.IndexOf(_captures, capture);
    if (_config.FACE_CAM.Contains(cameraIndex)) // 是用來臉部辨識
    {
      SetCameraResolution(capture, _config.AUTO_FRAME_WIDTH, _config.AUTO_FRAME_HEIGHT);
      Log.Information($"鏡頭{systemCameras[cameraIndex].DevicePath}畫面設定{capture.Width}*{capture.Height}p");
    }
    else
    {
      SetCameraResolution(capture, _config.FRAME_WIDTH, _config.FRAME_HEIGHT);
      Log.Information($"鏡頭{systemCameras[cameraIndex].DevicePath}畫面設定{_config.FRAME_WIDTH}*{_config.FRAME_HEIGHT}p");
    }
  }

  private void SetCameraResolution(VideoCapture capture, int width, int height)
  {
    capture.Set(Emgu.CV.CvEnum.CapProp.FrameWidth, width);
    capture.Set(Emgu.CV.CvEnum.CapProp.FrameHeight, height);

    // 如果設置的寬高為 0，則使用自動解析度
    if (capture.Width == 0 || capture.Height == 0)
    {
      capture.Set(Emgu.CV.CvEnum.CapProp.FrameWidth, _config.AUTO_FRAME_WIDTH);
      capture.Set(Emgu.CV.CvEnum.CapProp.FrameHeight, _config.AUTO_FRAME_HEIGHT);
      Log.Information($"鏡頭畫面設定為預設{_config.AUTO_FRAME_WIDTH}*{_config.AUTO_FRAME_HEIGHT}p");
    }
  }

  public async Task StartCapture(string cameraPath)
  {
    // 根據 cameraPath 獲取相應的攝影機索引
    int cameraIndex = Array.IndexOf(systemCameras.Select(c => c.DevicePath).ToArray(), cameraPath);

    if (cameraIndex < 0)
    {
      Log.Error($"找不到攝影機名稱: {cameraPath}");
      return;
    }

    var cts = new CancellationTokenSource();
    _cancellationTokenSources.Add(cts);
    var token = cts.Token;

    Log.Information($"開始抓取攝影機 {cameraPath} 畫面中...");

    while (!token.IsCancellationRequested)
    {
      Mat frame = new Mat();
      if (_captures[cameraIndex] != null && _captures[cameraIndex].IsOpened)
      {
        _captures[cameraIndex].Read(frame);
      }

      if (frame.IsEmpty)
      {
        Log.Warning($"攝像頭 {cameraPath} 無法讀取畫面，5秒後嘗試重新初始化。");
        _captures[cameraIndex]?.Dispose();
        await Task.Delay(5000, token); // Wait before retrying
        _captures[cameraIndex] = new VideoCapture(cameraIndex, _api);

        if (_captures[cameraIndex].IsOpened)
        {
          Log.Information($"攝像頭 {cameraPath} 重新連接成功。");
          ConfigureCamera(_captures[cameraIndex]);
        }
        else
        {
          Log.Error($"無法重新連接攝像頭 {cameraPath}。將再次重試。");
        }
        continue;
      }

      byte[] bytes = ImageConverter.ConvertFrameToBytes(frame);
      try
      {
        SendFramesToRedisAsync(bytes, _displayChannels[cameraIndex]);
      }
      catch (Exception ex)
      {
        Log.Error($"發送幀到 Redis 時發生錯誤: {ex.Message}");
      }

      await Dispatcher.UIThread.InvokeAsync(() =>
      {
        UpdateImage(cameraIndex, bytes);
      });

      await Task.Delay(100, token); // 控制帧率
    }
  }
  private void UpdateImage(int cameraIndex, byte[] imageData)
  {
    _uiManager.UpdateImage(cameraIndex, imageData);
  }
  private async void SendFramesToRedisAsync(byte[] frameBytes, string channel)
  {
    await _redisManager.PublishAsync(channel, frameBytes);
  }

  public void Dispose()
  {
    foreach (var capture in _captures)
    {
      capture.Dispose();
    }
  }
}