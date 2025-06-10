using StackExchange.Redis;
using Serilog;
using System;
using System.Threading.Tasks;
using Google.Protobuf;
using AvaloniaVideoMsg;
using Avalonia_redis;

public class Redismanager
{
  private ConnectionMultiplexer _redis;
  private ISubscriber _subscriber;
  private AppConfig _config;

  public Redismanager(AppConfig config)
  {
    _config = config;
    InitializeRedis();
  }

  private void InitializeRedis()
  {
    var options = new ConfigurationOptions
    {
      EndPoints = { _config.REDIS_SERVER_HOST + ":" + _config.REDIS_SERVER_PORT },
      Password = _config.REDIS_NEED_PASSWORD == 1 ? "3y)Z+0_A@P=n" : null
    };

    try
    {
      _redis = ConnectionMultiplexer.Connect(options);
      Log.Information("Redis 連接成功");
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

    _subscriber = _redis.GetSubscriber();
  }

  public async Task PublishAsync(string channelName, byte[] messageBytes)
  {
    var videoMsg = new VideoMsg
    {
      Cmd = "video",
      FrameData = ByteString.CopyFrom(messageBytes)
    };
    byte[] frameBytes = videoMsg.ToByteArray();
    await _subscriber.PublishAsync(channelName, frameBytes);
  }

  public void Dispose()
  {
    _redis.Dispose();
  }
}