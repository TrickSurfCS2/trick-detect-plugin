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

    private HudData? BuildHudData(Player player)
    {
        var client = player.Client;
        if (!Helpers.ClientIsValidAndAlive(client))
            return null;

        var hudData = new HudData
        {
            Speed = client.GetSpeed(),
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
        var client = player.Client;
        if (!Helpers.ClientIsValidAndAlive(client))
        {
            return;
        }

        var @event = new EventShowSurvivalRespawnStatus(false)
        {
            LocToken = hudContent,
            Duration = 5,
            Userid = client
        };
        @event.FireEvent(false);
        @event = null;
    }

    public void OnTickEvent(EventOnTickEvent e)
    {
        var players = playerManager.GetPlayerList();

        foreach (var player in players)
        {
            if (!player.ShowHud)
                continue;

            var hudData = BuildHudData(player);
            if (hudData == null)
                continue;

            var hudContent = RenderHud(hudData);
            TimerPrintHtml(player, hudContent);
        }
    }

}
