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
    public required int Slot { get; init; }
    public required string SteamId { get; init; }
    public required string Name { get; init; }
  }

  public Player(int Slot, string SteamId, string Name, Map Map)
  {
    Info = new PlayerInfo
    {
      Slot = Slot,
      SteamId = SteamId,
      Name = Name
    };
    SelectedMap = Map;
  }

  public readonly PlayerInfo Info;
  public Map SelectedMap { get; set; }

  public List<Location> SavedLocations { get; set; } = new();
  public int CurrentSavelocIndex { get; set; } = 0;
  public bool Teleporting { get; set; } = false;
  public Permission[] Permissions { get; set; } = [];
  public bool ShowHud { get; set; } = true;
  public bool Debug { get; set; } = false;

  // === Trick data === //
  public List<Trigger> Jumps { get; set; } = new();
  public List<Trigger> RouteTriggers { get; set; } = new();
  public double StartSpeed { get; set; } = 0.0;
  public StartType StartType { get; set; } = StartType.Velocity;
  // ================== //

  public CCSPlayerController Client => Utilities.GetPlayerFromSlot(Info.Slot)!;
  public string RouteTriggerPath => string.Join(",", RouteTriggers.Select(t => t.FullName));
  public bool IsJumped => Jumps.Count > 0;

  public void AddSavedLocation(Location location)
  {
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

  public void ResetTrickProgress()
  {
    Jumps = new();
    RouteTriggers = new();
    StartSpeed = 0.0;
  }
}
