using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Avalonia_redis.ViewModels;
using Avalonia_redis.Views;
using Emgu.CV;
using StackExchange.Redis;
using System;
using System.IO;
using System.Threading.Tasks;
using Google.Protobuf;
using AvaloniaVideoMsg;
using Serilog;
using System.ComponentModel;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using DirectShowLib;
namespace Avalonia_redis;


public partial class App : Avalonia.Application
{

	private List<CancellationTokenSource> _cancellationTokenSources = new List<CancellationTokenSource>();
	VideoCapture[] _captures;
	private Redismanager _redisManager;
	private UIManager _uiManager;
	public AppConfig _config;
	private CameraManager _cameraManager;
	DsDevice[] systemCameras = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice).OrderBy(d => d.DevicePath).ToArray();

	public override void Initialize()
	{
		string logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "log");
		if (!Directory.Exists(logDirectory))
		{
			Directory.CreateDirectory(logDirectory);
		}
		string logFilePath = Path.Combine(logDirectory, "log.txt");
		Log.Logger = new LoggerConfiguration().MinimumLevel.Debug().WriteTo.File(logFilePath, rollingInterval: RollingInterval.Day).CreateLogger();
		Log.Information("應用程式初始化");
		AvaloniaXamlLoader.Load(this);
		InitializeApp();


	}
	public override void OnFrameworkInitializationCompleted()
	{
		Log.Information("應用程式初始化中...");
		if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
		{
			var mainWindow = new MainWindow
			{
				DataContext = new MainViewModel()
			};
			mainWindow.Closing += MainWindow_Closing;
			mainWindow.WindowState = Avalonia.Controls.WindowState.Normal;
			desktop.MainWindow = mainWindow;
			_uiManager = new UIManager(_config);
			_uiManager.InitializeUI(mainWindow);
			string appNameWithExtension = $"{Path.GetFileNameWithoutExtension(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName)}.exe";
			string windowTitle = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, appNameWithExtension);
			desktop.MainWindow.Title = windowTitle;
			desktop.MainWindow.Topmost = true; // 置頂

		}
		else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
		{
			var mainView = new MainView
			{
				DataContext = new MainViewModel()
			};
			singleViewPlatform.MainView = mainView;
			singleViewPlatform.MainView.IsVisible = false;
			singleViewPlatform.MainView.Width = 0;
			singleViewPlatform.MainView.Height = 0;
		}
		_cameraManager = new CameraManager(_config, _config.VIDEO_CAPTURE_API, systemCameras, _uiManager, _redisManager);
		_cameraManager.InitializeCameras();
		Log.Information("應用程式初始化成功");

		base.OnFrameworkInitializationCompleted();
	}

	private void InitializeApp()
	{
		_captures = new VideoCapture[systemCameras.Length];
		string configFilePath = @"C:\deploy_item\config.json";
		try
		{
			//string configFilePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "deploy_item", "ai", "config.json");
			if (!File.Exists(configFilePath))
			{
				ConfigManager.GenerateConfigFile(configFilePath);
			}
			_config = ConfigManager.LoadConfig(configFilePath);
			Log.Information("config 設定成功");
		}
		catch (FileNotFoundException ex)
		{
			Log.Error($"找不到配置文件: {configFilePath}. 錯誤訊息: {ex.Message}");
		}
		catch (UnauthorizedAccessException ex)
		{
			Log.Error($"無法訪問配置文件: {configFilePath}. 錯誤訊息: {ex.Message}");
		}
		catch (Exception ex)
		{
			Log.Error($"處理配置文件時發生未知錯誤: {ex.Message}");
		}


		try
		{
			_redisManager = new Redismanager(_config);
			Log.Information("Redis 實例化");
		}
		catch (RedisConnectionException ex)
		{
			Log.Error($"Redis 連接失敗: {ex.Message}");
		}
		catch (TimeoutException ex)
		{
			Log.Error($"連接 Redis 時超時: {ex.Message}");
		}
		catch (Exception ex)
		{
			Log.Error($"Redis 連接發生未知錯誤: {ex.Message}");
		}

	}

	private void MainWindow_Closing(object? sender, CancelEventArgs e)
	{
		e.Cancel = true;
		Log.Information("應用程式正在關閉...");
		foreach (var cts in _cancellationTokenSources)
		{
			cts.Cancel();
		}
		var quitMsg = new VideoMsg
		{
			Cmd = "quit",
			FrameData = ByteString.Empty
		};
		byte[] messageBytes = quitMsg.ToByteArray();
		_redisManager.PublishAsync(_config.REDIS_VIDEO_DATA_CHAN, messageBytes);
		_redisManager.Dispose();
		for (int i = 0; i < systemCameras.Length; i++)
			_captures[i].Dispose();
		Log.Information("應用程式關閉完成");

		e.Cancel = false;
	}

}
