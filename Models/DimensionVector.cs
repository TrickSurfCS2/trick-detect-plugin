using CounterStrikeSharp.API.Modules.Utils;

namespace TrickDetect;

public class DimensionVector(float x, float y, float z)
{
  public float X { get; set; } = x;
  public float Y { get; set; } = y;
  public float Z { get; set; } = z;

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
