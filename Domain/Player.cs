using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace TrickDetect;

public class Player
{
  public class PlayerInfo
  {
    public required int Slot { get; init; }
    public required string SteamId { get; init; }
    public required string Name { get; init; }
  }

  public Player(int Slot, string SteamId, string Name)
  {
    Info = new PlayerInfo
    {
      Slot = Slot,
      SteamId = SteamId,
      Name = Name
    };
  }

  public readonly PlayerInfo Info;
  public List<Location> SavedLocations { get; set; } = new();
  public int CurrentSavelocIndex { get; set; } = 0;
  public bool Teleporting { get; set; } = false;
  public bool ShowHud { get; set; } = true;
  public CCSPlayerController Client => Utilities.GetPlayerFromSlot(Info.Slot)!;

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

}
