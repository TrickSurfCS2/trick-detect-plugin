namespace TrickDetect;

public class Trigger
{
  public required int Id { get; set; }
  public required string Name { get; set; }
  public required string FullName { get; set; }
  public required string PreviewImage { get; set; }
  public required DateTime CreatedAt { get; set; }
  public required DateTime UpdatedAt { get; set; }
}

public class RouteTrigger
{
  public required Trigger TouchedTrigger { get; set; }
  public required double SpeedEndTouch { get; set; }
  public required float? TimeStartTouch { get; set; }
  public required float? TimeEndTouch { get; set; }
  public required double? AvgSpeedStartTouch { get; set; } // Average speed from previous to current trigger
  public required double? AvgSpeedEndTouch { get; set; } // Average speed inside current trigger
}

public class TriggersTrick
{
  public required int TrickId { get; set; }
  public required int TriggerId { get; set; }
  public required string TriggerName { get; set; }
  public required string TriggerFullName { get; set; }
  public required string TriggerPreview { get; set; }
  public required DateTime TriggerCreatedAt { get; set; }
  public required DateTime TriggerUpdatedAt { get; set; }
}
