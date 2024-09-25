namespace TrickDetect;

public class EventOnTickEvent
{

}

public class EventSendAd { }

public class EventOnStartTouchEvent
{
  public required string? TriggerName { get; init; }
  public required Player Player { get; init; }
}

public class EventOnEndTouchEvent
{
  public required string? TriggerName { get; init; }
  public required Player Player { get; init; }
}

public class EventOnJump
{
  public required Player Player { get; init; }
}


public class EventOnPlayerConnect
{
  public required string SteamId { get; init; }
  public required int Slot { get; init; }
  public required string Name { get; init; }
}

public class EventOnPlayerDisconnect
{
  public required string SteamId { get; init; }
  public required int Slot { get; init; }
  public required string Name { get; init; }
}
