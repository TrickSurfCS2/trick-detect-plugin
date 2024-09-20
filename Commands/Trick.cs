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
  public async Task OnUpdateTrickAsync(CCSPlayerController client, CommandInfo _)
  {
    var pawn = client.PlayerPawn.Value;
    if (pawn == null)
      return;

    var player = _playerManager.GetPlayer(client);

    if (!player.Permissions.Contains(Permission.UpdateTricks))
    {
      player.Client.PrintToChat($" {ChatColors.Purple} You dont have permission for this command!");
      return;
    }

    await Task.Run(async () =>
    {
      var maps = _mapManager.GetAllMaps();
      foreach (var map in maps)
      {
        var tricks = await _trickManager.LoadMapTricks(map);
        _trickManager.SetTricksToMap(map, tricks);
      }

      Server.PrintToChatAll($" {ChatColors.Purple} All tricks have been updated");
    });
  }

  [ConsoleCommand("map", "Select map")]
  [CommandHelper(minArgs: 1, usage: "<map> ski2", whoCanExecute: CommandUsage.CLIENT_ONLY)]
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
    player.SelectedMap = map;
    player.Client.PlayerPawn.Value!.Teleport(
        map.Origin.ToVector(),
        null,
        null
    );
  }
}
