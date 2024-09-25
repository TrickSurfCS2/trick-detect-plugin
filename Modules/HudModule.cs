using CounterStrikeSharp.API.Core;
using TrickDetect.Managers;

namespace TrickDetect;

public class HudModule(PlayerManager playerManager)
{


    const string PRIMARY_COLOR = "#00DFA2";
    const string SECONDARY_COLOR = "#F6FA70";
    const string TERTIARY_COLOR = "#FF0060";
    const string WHITE = "#FFFFFF";

    private class HudData
    {
        public required double Speed { get; set; }
        public required double StartSpeed { get; set; }
    }

    private HudData BuildHudData(Player player)
    {
        var hudData = new HudData
        {
            Speed = Math.Round(player.Client.PlayerPawn.Value!.AbsVelocity.Length2D()),
            StartSpeed = player.StartSpeed,
        };

        return hudData;
    }


    private string RenderHud(HudData hudData)
    {
        var speedLine = $" <font class='fontSize-xl' color='{PRIMARY_COLOR}'>{hudData.Speed}</font> <br>";
        var startSpeedLine = $" <font class='fontSize-m' color='{SECONDARY_COLOR}'>{hudData.StartSpeed}</font> <br>";

        return speedLine + startSpeedLine;
    }

    private void TimerPrintHtml(Player player, string hudContent)
    {
        if (!player.ShowHud)
        {
            return;
        }
        var @event = new EventShowSurvivalRespawnStatus(false)
        {
            LocToken = hudContent,
            Duration = 5,
            Userid = player.Client
        };
        @event.FireEvent(false);
        @event = null;
    }

    public void OnTickEvent(EventOnTickEvent e)
    {
        var players = playerManager.GetPlayerList();

        foreach (var player in players)
        {
            var hudData = BuildHudData(player);
            var hudContent = RenderHud(hudData);
            TimerPrintHtml(player, hudContent);
        }
    }

}
