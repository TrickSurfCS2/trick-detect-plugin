namespace TrickDetect;

public class EventOnTickEvent { }

public class EventSendAd { }

public class BaseEventOn
{
  public required Player Player { get; init; }
}

public class EventOnStartTouchEvent : BaseEventOn
{
  public required string? TriggerName { get; init; }
}

public class EventOnEndTouchEvent : BaseEventOn
{
  public required string? TriggerName { get; init; }
}

public class EventOnJump : BaseEventOn { }
public class EventOnSpawn : BaseEventOn { }
public class EventOnDeath : BaseEventOn { }


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
