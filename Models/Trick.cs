namespace TrickDetect;

public class Trick
{
  public required int Id { get; set; }
  public required string Name { get; set; }
  public required int Point { get; set; }
  public required StartType StartType { get; set; }
  public required DateTime CreatedAt { get; set; }
  public required DateTime UpdatedAt { get; set; }
  public required List<Trigger> Triggers { get; set; }

  public string GetRouteTriggerPath()
  {
    return string.Join(",", Triggers.Select(t => t.Name));
  }
}
