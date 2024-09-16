namespace TrickDetect;

public partial class TrickDetect
{
  private static void SubscribeEvents()
  {
    var connectionHandler = new ConnectionModule(_database, _playerManager);
    var adHandler = new AdModule();
    var hudHandler = new HudModule(_playerManager);

    _eventsManager.Subscribe<EventOnPlayerConnect>(connectionHandler.OnPlayerConnect);
    _eventsManager.Subscribe<EventOnPlayerDisconnect>(connectionHandler.OnPlayerDisconnect);
    _eventsManager.Subscribe<EventSendAd>(adHandler.SendAdToChat);
    _eventsManager.Subscribe<EventOnTickEvent>(hudHandler.OnTickEvent);
  }
}
