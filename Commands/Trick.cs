using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;

namespace TrickDetect;

partial class TrickDetect
{
  [ConsoleCommand("update_tricks", "Update tricks list")]
  [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
  public void OnUpdateTrickAsync(CCSPlayerController client, CommandInfo _)
  {
    var pawn = client.PlayerPawn.Value;
    if (pawn == null)
      return;

    var player = _playerManager.GetPlayer(client);


    // TODO
    // if (!player.Permissions.Contains(Permission.UpdateTricks))
    // {
    // player.Client.PrintToChat($" {ChatColors.Purple} You dont have permission for this command!");
    // return;
    // }

    _mapManager.LoadAndSetAllMaps().Wait();
    var maps = _mapManager.GetAllMaps();
    this.LoadMapData(maps);

    Server.PrintToChatAll($" {ChatColors.Purple} All tricks have been updated");
  }

  [ConsoleCommand("m", "Select map")]
  [CommandHelper(minArgs: 1, usage: "<m> ski2", whoCanExecute: CommandUsage.CLIENT_ONLY)]
  public void OnSelectMap(CCSPlayerController client, CommandInfo command)
  {
    var pawn = client.PlayerPawn.Value;
    if (pawn == null || command.ArgCount < 2)
      return;

    var map = _mapManager.GetMapByName(command.GetArg(1));

    if (map == null)
    {
      client.PrintToChat($" {ChatColors.White}Cannot find map");
      return;
    }

    var player = _playerManager.GetPlayer(client);

    player.ResetTrickProgress();
    player.SavedLocations.Clear();
    player.SelectedMap = map;
    player.Client.PlayerPawn.Value!.Teleport(map.Origin.ToVector(), null, null);
  }

  [ConsoleCommand("tricks", "Show all tricks for map")]
  [CommandHelper(minArgs: 1, usage: "<tricks> ski2", whoCanExecute: CommandUsage.CLIENT_ONLY)]
  public void OnTricksMap(CCSPlayerController client, CommandInfo command)
  {
    var pawn = client.PlayerPawn.Value;
    if (pawn == null || command.ArgCount < 2)
      return;

    var map = _mapManager.GetMapByName(command.GetArg(1));
    var trickName = command.GetArg(2);

    if (map == null)
    {
      client.PrintToChat($" {ChatColors.White}Cannot find map");
      return;
    }

    var tricks = _trickManager.GetTricksByMap(map);

    if (trickName != null)
      tricks = tricks.Where(trick => trick.Name.Replace(" ", "_").ToLower().StartsWith(trickName)).ToArray();

    foreach (var trick in tricks)
    {
      client.PrintToConsole($"- {trick.Name} {trick.Point} {trick.StartType}");
      client.PrintToConsole($"> {trick.GetRouteTriggerPath()}");
    }
  }
}
