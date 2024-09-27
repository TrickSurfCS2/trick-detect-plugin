using TrickDetect.Modules;

namespace TrickDetect;

public partial class TrickDetect
{
  private static void SubscribeEvents()
  {
    var connectionHandler = new ConnectionModule(_playerManager, _mapManager);
    var trickRouteModule = new TrickRouteModule(_playerManager, _triggerManager, _trickManager);
    var hudHandler = new HudModule(_playerManager);
    var adHandler = new AdModule();

    _eventsManager.Subscribe<EventOnPlayerConnect>(connectionHandler.OnPlayerConnect);
    _eventsManager.Subscribe<EventOnPlayerDisconnect>(connectionHandler.OnPlayerDisconnect);

    _eventsManager.Subscribe<EventOnStartTouchEvent>(trickRouteModule.OnPlayerStartTouch);
    _eventsManager.Subscribe<EventOnEndTouchEvent>(trickRouteModule.OnPlayerEndTouch);
    _eventsManager.Subscribe<EventOnJump>(trickRouteModule.OnPlayerJump);
    _eventsManager.Subscribe<EventOnTickEvent>(trickRouteModule.OnTick);
    _eventsManager.Subscribe<EventOnSpawn>(trickRouteModule.OnPlayerSpawn);
    _eventsManager.Subscribe<EventOnDeath>(trickRouteModule.OnPlayerDeath);

    _eventsManager.Subscribe<EventOnTickEvent>(hudHandler.OnTickEvent);

    _eventsManager.Subscribe<EventSendAd>(adHandler.SendAdToChat);
  }
}
