namespace TrickDetect;

public class EventOnTickEvent
{

}

public class EventOnStartTouchEvent
{
  public string? TriggerName { get; init; }
  public int PlayerSlot { get; set; }
}

public class EventOnEndTouchEvent
{
  public string? TriggerName { get; init; }
  public int PlayerSlot { get; set; }
}


public class EventOnPlayerConnect
{
  public int? UserId { get; init; }
  public int Slot { get; init; }
  public string? SteamId3 { get; init; }
  public string? SteamId { get; init; }
  public string? Name { get; init; }
  public string? IpAddress { get; init; }

}

public class EventOnPlayerDisconnect
{
  public int? UserId { get; init; }
  public int Slot { get; init; }
  public string? SteamId3 { get; init; }
  public string? SteamId { get; init; }
  public string? Name { get; init; }
  public string? IpAddress { get; init; }

}
