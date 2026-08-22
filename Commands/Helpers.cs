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
    if (player == null)
      return;

    var newValue = !player.Debug;

    player.Debug = newValue;
    if (!newValue)
      player.DebugExtended = false;

    client.PrintToChat($" {ChatColors.Purple} Debug mode is {ChatColors.Grey} {(newValue ? "activated" : "deactivated")}");
  }

  [ConsoleCommand("debug_extended", "Toggle extended debug mode with live route candidates")]
  [ConsoleCommand("sm_debug_extended", "Toggle extended debug mode with live route candidates")]
  [ConsoleCommand("de", "Toggle extended debug mode with live route candidates")]
  [ConsoleCommand("sm_de", "Toggle extended debug mode with live route candidates")]
  [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
  public void OnDebugExtended(CCSPlayerController client, CommandInfo _)
  {
    var pawn = client.PlayerPawn.Value;
    if (pawn == null)
      return;

    var player = _playerManager.GetPlayer(client);
    if (player == null)
      return;

    var newValue = !player.DebugExtended;

    player.DebugExtended = newValue;
    if (newValue)
      player.Debug = true;

    client.PrintToChat($" {ChatColors.Purple} Extended debug mode is {ChatColors.Grey} {(newValue ? "activated" : "deactivated")}");
  }

  [ConsoleCommand("noclipme", "NoclipMe")]
  [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
  public void OnNoclipCommand(CCSPlayerController client, CommandInfo _)
  {
    var pawn = client.PlayerPawn.Value;
    if (pawn == null)
      return;

    var player = _playerManager.GetPlayer(client);
    if (player == null)
      return;

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

  [ConsoleCommand("info", "Print current player info")]
  [ConsoleCommand("sm_info", "Print current player info")]
  [ConsoleCommand("playerinfo", "Print current player info")]
  [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
  public void OnCurrentInfo(CCSPlayerController client, CommandInfo _)
  {
    var pawn = client.PlayerPawn.Value;
    if (pawn == null)
      return;

    var player = _playerManager.GetPlayer(client);
    if (player == null)
    {
      client.PrintToChat($" {ChatColors.Red}[TrickDetect] Player data not found.");
      return;
    }

    var mapName = player.SelectedMap?.Name ?? "None";
    int matchedCount = 0;
    if (player.SelectedMap != null)
    {
      var mapTricks = _trickManager.GetTricksByMap(player.SelectedMap);
      if (mapTricks?.allTricks != null)
      {
        matchedCount = _trickManager.CheckRouteTrickMatching(mapTricks.allTricks, player, out Trick? _);
      }
    }

    var path = string.IsNullOrEmpty(player.RouteTriggerPath) ? "None" : player.RouteTriggerPath;

    // Вывод в чат
    client.PrintToChat($" {ChatColors.Purple}=== [Player Info] ===");
    client.PrintToChat($" {ChatColors.Grey}Map: {ChatColors.Green}{mapName}{ChatColors.Grey} | Mode: {ChatColors.Yellow}{player.StartType}{ChatColors.Grey} | StartSpeed: {ChatColors.Yellow}{Math.Round(player.StartSpeed, 1)}");
    client.PrintToChat($" {ChatColors.Grey}Path: {ChatColors.LightBlue}{path}");
    client.PrintToChat($" {ChatColors.Grey}Matching tricks: {ChatColors.Gold}{matchedCount}{ChatColors.Grey} | Savelocs: {ChatColors.Gold}{player.SavedLocations.Count}");

    // Подробный вывод в консоль
    client.PrintToConsole($"=== [TrickDetect Info: {client.PlayerName}] ===");
    client.PrintToConsole($"SelectedMap: {mapName}");
    client.PrintToConsole($"StartType: {player.StartType}");
    client.PrintToConsole($"StartSpeed: {player.StartSpeed}");
    client.PrintToConsole($"IsJumped: {player.IsJumped}");
    client.PrintToConsole($"RouteTriggerPath: {path}");
    client.PrintToConsole($"RoutesMatched: {matchedCount}");
    client.PrintToConsole($"Permissions: {string.Join(",", player.Permissions)}");
    client.PrintToConsole($"RouteTriggers ({player.RouteTriggers.Count}):");
    foreach (var trigger in player.RouteTriggers)
    {
      client.PrintToConsole($"  - {trigger.TouchedTrigger.Name} | TimeStart: {trigger.TimeStartTouch} | TimeEnd: {trigger.TimeEndTouch} | Progress: Start={trigger.ProgressStartTouch?.Count ?? 0}, End={trigger.ProgressEndTouch?.Count ?? 0}");
    }
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
