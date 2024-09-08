using TrickDetect.Database;

namespace TrickDetect;

public partial class TrickDetect
{
  private void SubscribeEvents(DB database)
  {
    var connectionHandler = new ConnectionHandler(database);
    var adHandler = new AdHandler();

    if (_eventsObserver != null)
    {
      _eventsObserver.Subscribe<EventOnPlayerConnect>(connectionHandler.OnPlayerConnect);
      _eventsObserver.Subscribe<EventOnPlayerDisconnect>(connectionHandler.OnPlayerDisconnect);
      _eventsObserver.Subscribe<EventSendAd>(adHandler.SendAdToChat);
    }
  }
}
