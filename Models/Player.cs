using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace TrickDetect;

public enum StartType
{
  PreStrafe = 0,
  Velocity = 1,
}

public enum Permission
{
  UpdateTricks,
}

public class Player
{
  public class PlayerInfo
  {
    public required int Id { get; set; }
    public required string SteamId { get; set; }
    public required string Name { get; set; }
  }

  public class AverageSpeed
  {
    public required double Ticks { get; set; }
    public required double TotalSpeed { get; set; }
  }

  public Player(int slot, Map map)
  {
    Slot = slot;
    SelectedMap = map;
    AvgSpeedTicked = new AverageSpeed
    {
      Ticks = 0,
      TotalSpeed = 0.0f
    };
  }

  public int Slot { get; init; }
  public PlayerInfo? Info;
  public Map SelectedMap { get; set; }

  public List<Location> SavedLocations { get; set; } = new();
  public int CurrentSavelocIndex { get; set; } = 0;
  public bool Teleporting { get; set; } = false;
  public Permission[] Permissions { get; set; } = [];
  public bool ShowHud { get; set; } = true;
  public bool Debug { get; set; } = false;

  // === Trick data === //
  public List<RouteTrigger> RouteTriggers { get; set; } = new();
  public double StartSpeed { get; set; } = 0.0;
  public StartType StartType { get; set; } = StartType.Velocity;
  public bool IsJumped { get; set; } = false;
  public AverageSpeed AvgSpeedTicked { get; set; }
  public string RouteTriggerPath => string.Join(",", RouteTriggers.Select(t => t.TouchedTrigger.Name));
  public double AvgSpeed()
  {
    var speed = AvgSpeedTicked.TotalSpeed / AvgSpeedTicked.Ticks;
    ResetAverageSpeed();

    return speed;
  }
  // ================== //

  public CCSPlayerController Client => Utilities.GetPlayerFromSlot(Slot)!;

  public void SaveCurrentLocation()
  {
    var pawn = Client.PlayerPawn.Value!;

    var origin = new DimensionVector
    {
      X = pawn.AbsOrigin!.X,
      Y = pawn.AbsOrigin!.Y,
      Z = pawn.AbsOrigin!.Z
    };
    var angle = new DimensionVector
    {
      X = pawn.EyeAngles!.X,
      Y = pawn.EyeAngles!.Y,
      Z = pawn.EyeAngles!.Z
    };
    var velocity = new DimensionVector
    {
      X = pawn.AbsVelocity!.X,
      Y = pawn.AbsVelocity!.Y,
      Z = pawn.AbsVelocity!.Z
    };

    var location = new Location
    {
      origin = origin,
      angle = angle,
      velocity = velocity,
    };

    SavedLocations.Add(location);
    CurrentSavelocIndex = SavedLocations.Count - 1;
  }

  public void TeleportToSavedLocation()
  {
    if (SavedLocations.Count == 0)
    {
      Client.PrintToChat($"{ChatColors.White}No saveloc's");
      return;
    }

    var location = SavedLocations[CurrentSavelocIndex];

    Teleporting = true;
    Client.PlayerPawn.Value!.Teleport(
        location.origin.ToVector(),
        location.angle.ToQAngle(),
        location.velocity.ToVector()
    );

    Server.NextFrame(() =>
    {
      Teleporting = false;
    });
  }

  public void ResetAverageSpeed()
  {
    if (AvgSpeedTicked.Ticks == 0)
      return;

    AvgSpeedTicked = new AverageSpeed
    {
      Ticks = 0,
      TotalSpeed = 0.0f
    };
  }

  public void SetupStartSpeed()
  {
    var speed = Client.GetSpeed();
    StartSpeed = speed;
    StartType = speed < 400
        ? StartType.PreStrafe
        : StartType.Velocity;
  }

  public void ResetTrickProgress()
  {
    IsJumped = false;
    RouteTriggers = new();
    StartSpeed = 0.0;
    StartType = StartType.Velocity;
    ResetAverageSpeed();
  }
}
