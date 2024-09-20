using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;

namespace TrickDetect;

partial class TrickDetect
{
  [ConsoleCommand("debug", "Toggle debug mode")]
  [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
  public void OnDebug(CCSPlayerController client, CommandInfo _)
  {
    var pawn = client.PlayerPawn.Value;
    if (pawn == null)
      return;

    var player = _playerManager.GetPlayer(client);
    var newValue = !player.Debug;

    player.Debug = newValue;
    player.Client.PrintToChat($" {ChatColors.Purple} Debug mode is {ChatColors.Grey} {(newValue ? "activated" : "deactivated")}");
  }
}
