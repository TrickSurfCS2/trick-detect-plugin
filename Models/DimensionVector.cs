using CounterStrikeSharp.API.Modules.Utils;

namespace TrickDetect;

public class Location
{
  public required DimensionVector origin { get; set; }
  public required DimensionVector angle { get; set; }
  public required DimensionVector velocity { get; set; }
}

public class DimensionVector
{
  public required float X { get; set; }
  public required float Y { get; set; }
  public required float Z { get; set; }

  public Vector ToVector()
  {
    return new Vector(X, Y, Z);
  }

  public QAngle ToQAngle()
  {
    return new QAngle(X, Y, Z);
  }

  public override string ToString()
  {
    return $"{X} {Y} {Z}";
  }
}
