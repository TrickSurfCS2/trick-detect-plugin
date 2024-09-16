using CounterStrikeSharp.API.Core;
using TrickDetect.Managers;

namespace TrickDetect;

public class HudModule(PlayerManager playerManager)
{
    private readonly PlayerManager _playerManager = playerManager;

    const string PRIMARY_COLOR = "#00DFA2";

    private class HudData
    {
        public double Speed { get; set; }
    }

    private HudData BuildHudData(CCSPlayerController client)
    {
        var speed = Math.Round(client.PlayerPawn.Value!.AbsVelocity.Length2D());

        var hudData = new HudData
        {
            Speed = speed,
        };

        return hudData;
    }


    private string RenderHud(HudData hudData)
    {
        var speedLine = $" <font class='fontSize-xl' color='{PRIMARY_COLOR}'>{hudData.Speed}</font> <br>";

        return speedLine;
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
        var entities = _playerManager.GetPlayerList();

        foreach (var entity in entities)
        {
            var hudData = BuildHudData(entity.Client);
            var hudContent = RenderHud(hudData);
            TimerPrintHtml(entity, hudContent);
        }
    }

}
