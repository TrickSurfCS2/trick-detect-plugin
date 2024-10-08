using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using TrickDetect.Managers;

namespace TrickDetect.Modules;

public class TrickRouteModule(PlayerManager playerManager, TriggerManager triggerManager, TrickManager trickManager)
{
    public void OnPlayerStartTouch(EventOnStartTouchEvent e)
    {
        var player = e.Player;
        var triggerName = e.TriggerName;
        var mapTriggers = triggerManager.GetTriggersByMap(player.SelectedMap);
        var trigger = mapTriggers.FirstOrDefault((trigger) => trigger.Name == triggerName);

        if (player.Debug)
            player.Client.PrintToChat($"{ChatColors.Grey} StartTouch - {ChatColors.Purple} {triggerName} {trigger?.Id.ToString() ?? "❌"}");

        if (triggerName!.StartsWith("boost"))
        {
            int[] velocity = triggerName.Split("-")[1].Split("_")
                            .Select(int.Parse)
                            .ToArray();

            Server.NextFrame(() =>
            {
                player.Client.PlayerPawn.Value!.Teleport(
                    null,
                    null,
                    new Vector(velocity[0], velocity[1], velocity[2])
                );
                player.StartType = StartType.Velocity;
            });
        }

        if (trigger == null || player.Client.Pawn.Value!.MoveType == MoveType_t.MOVETYPE_NOCLIP)
            return;

        var routeTrigger = new RouteTrigger
        {
            TouchedTrigger = trigger,
            TimeStartTouch = Server.CurrentTime,
            TimeEndTouch = null,
            ProgressStartTouch = player.PlayerProgressData,
            ProgressEndTouch = null
        };
        player.PlayerProgressData = [];
        player.RouteTriggers.Add(routeTrigger);

        trickManager.RouteChecker(player);
    }

    public void OnPlayerEndTouch(EventOnEndTouchEvent e)
    {
        var player = e.Player;
        var triggerName = e.TriggerName;
        var mapTriggers = triggerManager.GetTriggersByMap(player.SelectedMap);
        var trigger = mapTriggers.FirstOrDefault((trigger) => trigger.Name == triggerName);

        if (player.Debug)
            player.Client.PrintToChat($"{ChatColors.Grey} StartTouch - {ChatColors.Purple} {triggerName} {trigger?.Id.ToString() ?? "❌"}");

        if (trigger == null || player.Client.Pawn.Value!.MoveType == MoveType_t.MOVETYPE_NOCLIP)
            return;

        if (player.RouteTriggers.Count == 0)
        {
            var routeTrigger = new RouteTrigger
            {
                TouchedTrigger = trigger,
                TimeStartTouch = null,
                TimeEndTouch = Server.CurrentTime,
                ProgressEndTouch = player.PlayerProgressData,
                ProgressStartTouch = null
            };

            // Если игрок решил не прыгать но покинул стартовую зону
            if (!player.IsJumped)
                player.SetupStartSpeed();

            player.RouteTriggers.Add(routeTrigger);
        }
        else
        {
            var previousTrigger = player.RouteTriggers.Find(f => f.TouchedTrigger.Id == trigger.Id);

            // Проверить если StartTouch сработал ранее чем EndTouch
            if (previousTrigger == null || previousTrigger.TouchedTrigger.Name != trigger.Name)
                return;

            previousTrigger.TimeEndTouch ??= Server.CurrentTime;
            previousTrigger.ProgressEndTouch ??= player.PlayerProgressData;
        }
        player.PlayerProgressData = [];
    }

    public void OnPlayerJump(EventOnJump e)
    {
        var player = e.Player;

        if (player.RouteTriggers.Count == 0)
        {
            player.SetupStartSpeed();
            player.IsJumped = true;
        }

        if (player.Debug)
            player.Client.PrintToConsole($"OnEventPlayerJump");
    }

    public void OnTick(EventOnTickEvent _)
    {
        var players = playerManager.GetPlayerList();

        foreach (var player in players)
        {
            if (!string.IsNullOrEmpty(player.RouteTriggerPath) && player.PlayerProgressData.Count < 1000000)
                player.CollectPlayerProgress();
            else if (player.PlayerProgressData.Count > 0)
                player.PlayerProgressData = [];
        }
    }

    public void OnPlayerSpawn(EventOnSpawn e)
    {
        var player = e.Player;

        player.ResetTrickProgress();
        player.Client.HideLegs();
    }

    public void OnPlayerDeath(EventOnDeath e)
    {
        var player = e.Player;

        player.ResetTrickProgress();
    }
}
