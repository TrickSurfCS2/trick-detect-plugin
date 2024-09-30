using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;

namespace TrickDetect;

public enum StartType
{
  PreStrafe = 0,
  Velocity = 1,
}

public enum Permission
{
  UpdateTrick,
  CreateTrick,
  DeleteTrick,
  BanUser,
  Teleport
}

public class Player
{
  public class PlayerInfo
  {
    public required int Id { get; set; }
    public required string SteamId { get; set; }
    public required string Name { get; set; }
  }

  public class PlayerProgress
  {
    public required double Speed { get; set; }
    public required Location Location { get; set; }
  }

  public Player(int slot, Map map)
  {
    Slot = slot;
    SelectedMap = map;
    SavedLocations = new();
    RouteTriggers = new();
    PlayerProgressData = new();
    Permissions = [];
    CurrentSavelocIndex = 0;
    StartSpeed = 0.0;
    StartType = StartType.Velocity;
  }

  public int Slot { get; init; }
  public PlayerInfo? Info;
  public Map SelectedMap { get; set; }

  public List<Location> SavedLocations { get; set; }
  public int CurrentSavelocIndex { get; set; }
  public bool Teleporting { get; set; } = false;
  public Permission[] Permissions { get; set; }
  public bool ShowHud { get; set; } = true;
  public bool Debug { get; set; } = false;

  // === Trick data === //
  public List<RouteTrigger> RouteTriggers { get; set; }
  public List<PlayerProgress> PlayerProgressData { get; set; }
  public double StartSpeed { get; set; }
  public StartType StartType { get; set; }
  public bool IsJumped { get; set; } = false;
  public string RouteTriggerPath => RouteTriggers.Any() ? string.Join(",", RouteTriggers.Select(t => t.TouchedTrigger.Name)) : string.Empty;
  // ================== //

  public CCSPlayerController Client => Utilities.GetPlayerFromSlot(Slot)!;

  public void SaveCurrentLocation()
  {
    var location = Client.GetLocation();
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

    ResetTrickProgress();

    Client.PlayerPawn.Value!.Teleport(
      location.origin.ToVector(),
      location.angle.ToQAngle(),
      location.velocity.ToVector()
    );

    if (Debug)
      Client.PrintToChat($"{ChatColors.Grey}Teleported");
  }

  private void AddTimer(float v, Func<object> value, object rEPEAT)
  {
    throw new NotImplementedException();
  }

  public void CollectPlayerProgress()
  {
    var location = Client.GetLocation();
    var progress = new PlayerProgress
    {
      Location = location,
      Speed = Client.GetSpeed()
    };

    PlayerProgressData.Add(progress);
  }

  public void SetupStartSpeed()
  {
    var speed = Client.GetSpeed();
    var maxSpeed = TrickDetect._cfg!.PreSpeed;

    StartSpeed = speed;
    StartType = speed < maxSpeed
        ? StartType.PreStrafe
        : StartType.Velocity;
  }

  public void ResetTrickProgress()
  {
    IsJumped = false;
    RouteTriggers.Clear();
    PlayerProgressData.Clear();

    SetupStartSpeed();
  }
}
