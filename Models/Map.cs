namespace TrickDetect;

public class Map
{
  public required int Id { get; set; }
  public required string Name { get; set; }
  public required string FullName { get; set; }
  public required string PreviewImage { get; set; }
  public required DimensionVector Origin { get; set; }
  public required DateTime CreatedAt { get; set; }
  public required DateTime UpdatedAt { get; set; }
}
