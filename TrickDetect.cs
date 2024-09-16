using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Modules.Timers;
using Microsoft.Extensions.Logging;
using Npgsql;
using TrickDetect.Config;
using TrickDetect.Database;
using TrickDetect.Managers;

namespace TrickDetect;

[MinimumApiVersion(260)]
public partial class TrickDetect : BasePlugin, IPluginConfig<TrickDetectConfig>
{
  public override string ModuleName => "TrickDetect Plugin";
  public override string ModuleVersion => "0.0.1";
  public override string ModuleAuthor => "injurka";

  public static TrickDetect Instance { get; private set; } = new();
  public TrickDetectConfig Config { get; set; } = new();

  private static PlayerManager _playerManager = new();
  private static EventManager _eventsManager = new();
  private static DB _database = new(null);
  internal static ILogger? _logger;

  public override void Load(bool hotReload)
  {
    Instance = this;
    _logger = Logger;

    _playerManager = new PlayerManager();
    _eventsManager = new EventManager();

    SubscribeEvents();
    RegisterEvents();

    AddTimer(45f, () =>
    {
      _eventsManager.Publish(new EventSendAd());
    }, TimerFlags.REPEAT);

    if (hotReload)
    {
      // TODO: Handle hot reload logic
    }
  }

  public override void Unload(bool hotReload)
  {
    if (hotReload) return;

    // TODO: Handle unload logic
  }

  public void OnConfigParsed(TrickDetectConfig config)
  {
    if (config.DatabaseHost.Length < 1 || config.DatabaseName.Length < 1 || config.DatabaseUser.Length < 1)
    {
      throw new Exception("[TrickDetect] You need to setup Database credentials in config!");
    }

    var builder = new NpgsqlConnectionStringBuilder
    {
      Host = config.DatabaseHost,
      Database = config.DatabaseName,
      Username = config.DatabaseUser,
      Password = config.DatabasePassword,
      Port = config.DatabasePort,
      Pooling = true,
      MinPoolSize = 0,
      MaxPoolSize = 640,
    };

    _database = new DB(builder.ConnectionString);

    if (!_database.CheckDatabaseConnection())
    {
      Logger.LogError("Unable to connect to database!");
      Unload(false);
      return;
    }

    Config = config;
  }
}
