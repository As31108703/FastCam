using System.IO;
using System.Text.Json;

namespace Avalonia_redis;

public class AppConfig
{
  public string APP_NAME { get; set; }
  public int REDIS_NEED_PASSWORD { get; set; }
  public int SHOW_UI { get; set; }
  public int FRAME_WIDTH { get; set; }
  public int FRAME_HEIGHT { get; set; }
  public int AUTO_FRAME_WIDTH { get; set; }
  public int AUTO_FRAME_HEIGHT { get; set; }
  public string DISPLAY_ONE { get; set; }
  public string DISPLAY_TWO { get; set; }
  public string DISPLAY_THREE { get; set; }
  public string DISPLAY_FOUR { get; set; }
  public string DISPLAY_FIVE { get; set; }
  public string DISPLAY_SIX { get; set; }
  public string DISPLAY_SEVEN { get; set; }
  public int[] FACE_CAM { get; set; }
  public string REDIS_VIDEO_DATA_CHAN { get; set; }
  public string REDIS_SERVER_HOST { get; set; }
  public int REDIS_SERVER_PORT { get; set; }
  public string VIDEO_CAPTURE_API { get; set; }
  public int MAX_CAM_COUNT { get; set; }
  public int GRID_ROWS { get; set; }
  public int GRID_COLUMNS { get; set; }
}

public static class ConfigManager
{
  public static void GenerateConfigFile(string fileName)
  {
    var currentDirectory = Directory.GetCurrentDirectory();
    var filePath = System.IO.Path.Combine(currentDirectory, fileName);

    var config = new AppConfig
    {
      APP_NAME = "Eyes Of Amaze AI",
      REDIS_NEED_PASSWORD = 0,
      FRAME_WIDTH = 1920,
      FRAME_HEIGHT = 1080,
      DISPLAY_ONE = "monitor_01",
      DISPLAY_TWO = "monitor_02",
      DISPLAY_THREE = "monitor_03",
      DISPLAY_FOUR = "monitor_04",
      DISPLAY_FIVE = "monitor_05",
      DISPLAY_SIX = "monitor_06",
      DISPLAY_SEVEN = "monitor_07",
      FACE_CAM = new int[] { 0, 5 },
      REDIS_VIDEO_DATA_CHAN = "unity_topics",
      SHOW_UI = 0,
      REDIS_SERVER_HOST = "127.0.0.1",
      REDIS_SERVER_PORT = 6379,
      VIDEO_CAPTURE_API = "DShow",
      AUTO_FRAME_WIDTH = 640,
      AUTO_FRAME_HEIGHT = 480,
      MAX_CAM_COUNT = 16,
      GRID_ROWS = 4,
      GRID_COLUMNS = 4,
    };

    var options = new JsonSerializerOptions
    {
      WriteIndented = true
    };

    var json = JsonSerializer.Serialize(config, options);
    File.WriteAllText(filePath, json);
  }

  public static AppConfig LoadConfig(string filePath)
  {
    if (!File.Exists(filePath))
    {
      throw new FileNotFoundException("Config file not found.");
    }

    var json = File.ReadAllText(filePath);
    var config = JsonSerializer.Deserialize<AppConfig>(json);
    return config;
  }
}