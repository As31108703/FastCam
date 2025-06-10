using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Serilog;
using System;
using System.IO;

namespace Avalonia_redis
{
  public class UIManager
  {
    private Canvas? _fullscreenContainer;
    private Image? _fullscreenImage;
    private bool _isFullscreen = false;
    private Image[] imgCameraPreview;
    private AppConfig _config;

    public UIManager(AppConfig config)
    {
      _config = config;
      imgCameraPreview = new Image[_config.MAX_CAM_COUNT];
    }

    public void InitializeUI(Control control)
    {
      Log.Information("應用程式UI初始化中...");
      _fullscreenContainer = control.FindControl<Canvas>("FullscreenContainer");
      _fullscreenImage = control.FindControl<Image>("FullscreenImage");
      _fullscreenImage.PointerPressed += Image_OnClick;
      _fullscreenContainer.SizeChanged += FullscreenContainer_SizeChanged;

      var cameraGrid = control.FindControl<UniformGrid>("CameraGrid");
      cameraGrid.Rows = _config.GRID_ROWS;
      cameraGrid.Columns = _config.GRID_COLUMNS;

      for (int i = 0; i < _config.MAX_CAM_COUNT; i++)
      {
        imgCameraPreview[i] = new Image
        {
          Name = $"imgCameraPreview{i + 1}",
          Stretch = Stretch.UniformToFill,
          HorizontalAlignment = HorizontalAlignment.Stretch,
          VerticalAlignment = VerticalAlignment.Stretch
        };
        imgCameraPreview[i].PointerPressed += Image_OnClick;
        cameraGrid.Children.Add(imgCameraPreview[i]);
      }

      Log.Information("應用程式UI初始化完成");
    }

    private void FullscreenContainer_SizeChanged(object? sender, SizeChangedEventArgs e)
    {
      if (_fullscreenImage == null || !_fullscreenContainer.IsVisible) return;
      if (_isFullscreen)
      {
        _fullscreenImage.Width = _fullscreenContainer.Bounds.Width;
        _fullscreenImage.Height = _fullscreenContainer.Bounds.Height;
        Log.Information("全屏大小改變" + _fullscreenImage.Width + "/" + _fullscreenImage.Height);
      }
    }

    private void Image_OnClick(object? sender, PointerPressedEventArgs e)
    {
      if (_fullscreenContainer == null || _fullscreenImage == null) return;

      if (sender is not Image clickedImage) return;

      if (!_isFullscreen)
      {
        // 進入全屏模式
        _fullscreenImage.Width = _fullscreenContainer.Bounds.Width;
        _fullscreenImage.Height = _fullscreenContainer.Bounds.Height;
        Log.Information("螢幕寬_fullscreenImage" + _fullscreenContainer.Bounds.Width);
        Log.Information("螢幕長_fullscreenImage" + _fullscreenContainer.Bounds.Height);
        _fullscreenImage.Bind(Image.SourceProperty, clickedImage[!Image.SourceProperty]);
        _fullscreenContainer.IsVisible = true;
      }
      else
      {
        // 退出全屏模式
        _fullscreenContainer.IsVisible = false;
        _fullscreenImage.ClearValue(Image.SourceProperty);
        _fullscreenImage.Source = null; // 清空全屏圖片
      }

      _isFullscreen = !_isFullscreen;
    }

    public void UpdateImage(int cameraIndex, byte[] imageData)
    {
      using (MemoryStream memoryStream = new MemoryStream(imageData))
      {
        Bitmap bitmap = new Bitmap(memoryStream);
        imgCameraPreview[cameraIndex].Source = bitmap;
      }
    }
  }
}