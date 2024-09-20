using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Modules.Timers;
using Microsoft.Extensions.Logging;
using TrickDetect.Database;
using TrickDetect.Managers;

namespace TrickDetect;

[MinimumApiVersion(260)]
public partial class TrickDetect : BasePlugin, IPluginConfig<TrickDetectConfig>
{
  public override string ModuleName => "TrickDetect Plugin";
  public override string ModuleVersion => "0.0.1";
  public override string ModuleAuthor => "injurka";

  private static DB _database = new(null);
  private static PlayerManager _playerManager = new(_database!);
  private static MapManager _mapManager = new(_database!);
  private static TriggerManager _tiggerManager = new(_database!);
  private static TrickManager _trickManager = new(_database!);
  private static EventManager _eventsManager = new();
  internal static ILogger? _logger;

  public override void Load(bool hotReload)
  {
    _logger = Logger;

    _playerManager = new PlayerManager(_database);
    _tiggerManager = new TriggerManager(_database);
    _trickManager = new TrickManager(_database);
    _mapManager = new MapManager(_database);
    _eventsManager = new EventManager();

    SubscribeEvents();
    RegisterEvents();

    AddTimer(45f, () => _eventsManager.Publish(new EventSendAd()), TimerFlags.REPEAT);
  }

  public override void Unload(bool hotReload)
  {
    if (hotReload)
      return;

    UnRegisterEvents();
  }
}
