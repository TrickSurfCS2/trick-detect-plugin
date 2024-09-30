using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using Microsoft.Extensions.Logging;
using TrickDetect.Database;
using TrickDetect.Managers;

namespace TrickDetect;

[MinimumApiVersion(264)]
public partial class TrickDetect : BasePlugin, IPluginConfig<TrickDetectConfig>
{
  public override string ModuleName => "TrickDetect Plugin";
  public override string ModuleVersion => "1.1.0";
  public override string ModuleAuthor => "injurka";

  private static DB _database = new(null);
  private static PlayerManager _playerManager = new(_database!);
  private static MapManager _mapManager = new(_database!);
  private static TriggerManager _triggerManager = new(_database!);
  private static TrickManager _trickManager = new(_database!);
  private static EventManager _eventsManager = new();
  internal static ILogger? _logger;
  internal static TrickDetectConfig? _cfg;

  public override void Load(bool hotReload)
  {
    _logger = Logger;
    _cfg = Config;

    _mapManager = new MapManager(_database);
    _triggerManager = new TriggerManager(_database);
    _trickManager = new TrickManager(_database);
    _playerManager = new PlayerManager(_database);
    _eventsManager = new EventManager();

    _mapManager.LoadAndSetAllMaps().Wait();
    var maps = _mapManager.GetAllMaps();

    LoadMapData(maps);
    SubscribeEvents();
    RegisterEvents();
  }

  public override void Unload(bool hotReload)
  {
    if (hotReload)
      return;
  }

  public void LoadMapData(Map[] maps)
  {
    Logger.LogInformation($"Load maps {maps.Count()}");
    foreach (var map in maps)
    {
      Logger.LogInformation($"Load data for map {map.FullName}");
      _triggerManager.LoadAndSetMapTriggers(map).Wait();
      _trickManager.LoadAndSetMapTricks(map).Wait();
      Logger.LogInformation($"Loaded all data");
      Logger.LogInformation($"Tricks counts {_trickManager.GetTricksByMap(map).allTricks.Count()}");
      foreach (var item in _trickManager.GetTricksByMap(map).tricksRoutesContain)
        Logger.LogInformation($"Tricks paths {item.Key} {item.Value.Count()}");
      foreach (var item in _trickManager.GetTricksByMap(map).tricksRoutesUnique)
        Logger.LogInformation($"Tricks unique paths {item.Value.Count()}");
      Logger.LogInformation($"Triggers counts {_triggerManager.GetTriggersByMap(map).Count()}");
    }
  }
}
