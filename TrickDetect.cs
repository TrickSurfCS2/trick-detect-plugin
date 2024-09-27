using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Modules.Timers;
using Microsoft.Extensions.Logging;
using TrickDetect.Database;
using TrickDetect.Managers;

namespace TrickDetect;

[MinimumApiVersion(264)]
public partial class TrickDetect : BasePlugin, IPluginConfig<TrickDetectConfig>
{
  public override string ModuleName => "TrickDetect Plugin";
  public override string ModuleVersion => "1.0.0";
  public override string ModuleAuthor => "injurka";

  private static DB _database = new(null);
  private static PlayerManager _playerManager = new(_database!);
  private static MapManager _mapManager = new(_database!);
  private static TriggerManager _triggerManager = new(_database!);
  private static TrickManager _trickManager = new(_database!);
  private static EventManager _eventsManager = new();
  internal static ILogger? _logger;

  public override void Load(bool hotReload)
  {
    _logger = Logger;

    _mapManager = new MapManager(_database);
    _triggerManager = new TriggerManager(_database);
    _trickManager = new TrickManager(_database);
    _playerManager = new PlayerManager(_database);
    _eventsManager = new EventManager();

    _mapManager.LoadAndSetAllMaps().Wait();
    var maps = _mapManager.GetAllMaps();

    foreach (var map in maps)
    {
      Logger.LogInformation($"Load data for map {map.FullName}");
      _triggerManager.LoadAndSetMapTriggers(map).Wait();
      Logger.LogInformation($"Loaded triggers {_triggerManager.GetTriggersByMap(map).Count()}");
      _trickManager.LoadAndSetMapTricks(map).Wait();
      Logger.LogInformation($"Loaded tricks {_trickManager.GetTricksByMap(map).Count()}");
    }

    SubscribeEvents();
    RegisterEvents();
  }

  public override void Unload(bool hotReload)
  {
    if (hotReload)
      return;
  }
}
