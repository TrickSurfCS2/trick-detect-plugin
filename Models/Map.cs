namespace TrickDetect;

public class Map
{
  public int Id { get; set; }
  public string Name { get; set; }
  public string FullName { get; set; }
  public string PreviewImage { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
  public DimensionVector Origin { get; set; }

  public Map(int id, string name, string fullName, string previewImage, DateTime createdAt, DateTime updatedAt, float[] origin)
  {
    Id = id;
    Name = name;
    FullName = fullName;
    PreviewImage = previewImage;
    CreatedAt = createdAt;
    UpdatedAt = updatedAt;
    Origin = new DimensionVector { X = origin[0], Y = origin[1], Z = origin[2] };
  }
}
