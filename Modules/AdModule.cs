using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Utils;

namespace TrickDetect;

public class AdModule
{
    public void SendAdToChat(EventSendAd _)
    {
        Server.NextFrame(() =>
        {
            Server.PrintToChatAll($" {ChatColors.Gold} SurfGxds {ChatColors.Blue} Приветствуем вас воин!");
        });
    }
}
