using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Memory;
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

  [ConsoleCommand("noclipme", "NoclipMe")]
  [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
  public void OnNoclipCommand(CCSPlayerController client, CommandInfo _)
  {
    var pawn = client.PlayerPawn.Value;
    if (pawn == null)
      return;

    var player = _playerManager.GetPlayer(client);

    if (pawn.MoveType == MoveType_t.MOVETYPE_NOCLIP)
    {
      pawn.MoveType = MoveType_t.MOVETYPE_WALK;
      Schema.SetSchemaValue(pawn.Handle, "CBaseEntity", "m_nActualMoveType", 2);
      Utilities.SetStateChanged(pawn, "CBaseEntity", "m_MoveType");
    }
    else
    {
      pawn.MoveType = MoveType_t.MOVETYPE_NOCLIP;
      Schema.SetSchemaValue(pawn.Handle, "CBaseEntity", "m_nActualMoveType", 8);
      Utilities.SetStateChanged(pawn, "CBaseEntity", "m_MoveType");
    }

    player.ResetTrickProgress();
  }

  [ConsoleCommand("info", "Print current info player")]
  [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
  public void OnCurrentInfo(CCSPlayerController client, CommandInfo _)
  {
    var pawn = client.PlayerPawn.Value;
    if (pawn == null)
      return;

    var player = _playerManager.GetPlayer(client);
    Trick[] tricks = _trickManager.GetTricksByMap(player.SelectedMap);

    player.Client.PrintToConsole($"RouteTriggerPath >>> {player.RouteTriggerPath}");
    player.Client.PrintToConsole($"SelectedMap >>> {player.SelectedMap.Name}");
    player.Client.PrintToConsole($"StartType >>> {player.StartType}");
    player.Client.PrintToConsole($"StartSpeed >>> {player.StartSpeed}");
    player.Client.PrintToConsole($"IsJumped >>> {player.IsJumped}");
    player.Client.PrintToConsole($"RoutesMatched >>> {_trickManager.CheckRouteTrickMatching(tricks, player, out Trick? _)}");
  }


  [ConsoleCommand("fov", "Sets the player's FOV")]
  [CommandHelper(minArgs: 1, usage: "[fov]", whoCanExecute: CommandUsage.CLIENT_ONLY)]
  public void OnFovCommand(CCSPlayerController client, CommandInfo command)
  {
    var pawn = client.PlayerPawn.Value;
    if (pawn == null)
      return;

    if (!Int32.TryParse(command.GetArg(1), out var desiredFov)) return;

    client.DesiredFOV = (uint)desiredFov;
    Utilities.SetStateChanged(client, "CBasePlayerController", "m_iDesiredFOV");
  }
}
