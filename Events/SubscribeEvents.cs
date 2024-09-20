using TrickDetect.Modules;

namespace TrickDetect;

public partial class TrickDetect
{
  private static void SubscribeEvents()
  {
    var connectionHandler = new ConnectionModule(_playerManager, _mapManager);
    var triggerTouchModule = new TriggerTouchModule(_triggerManager, _trickManager);
    var hudHandler = new HudModule(_playerManager);
    var adHandler = new AdModule();

    _eventsManager.Subscribe<EventOnPlayerConnect>(connectionHandler.OnPlayerConnect);
    _eventsManager.Subscribe<EventOnPlayerDisconnect>(connectionHandler.OnPlayerDisconnect);

    _eventsManager.Subscribe<EventOnStartTouchEvent>(triggerTouchModule.OnPlayerStartTouch);
    _eventsManager.Subscribe<EventOnEndTouchEvent>(triggerTouchModule.OnPlayerEndTouch);

    _eventsManager.Subscribe<EventOnTickEvent>(hudHandler.OnTickEvent);

    _eventsManager.Subscribe<EventSendAd>(adHandler.SendAdToChat);
  }
}
