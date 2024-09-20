using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;

namespace TrickDetect;

partial class TrickDetect
{
  [ConsoleCommand("hud", "Toggle hud visibility")]
  [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
  public void OnHud(CCSPlayerController client, CommandInfo _)
  {
    var pawn = client.PlayerPawn.Value;
    if (pawn == null)
      return;

    var player = _playerManager.GetPlayer(client);
    var newValue = !player.ShowHud;

    player.ShowHud = newValue;
    player.Client.PrintToChat($" {ChatColors.Purple} Hud is {ChatColors.Grey} {(newValue ? "showed" : "hidden")}");
  }
}
