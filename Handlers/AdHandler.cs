using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Utils;

namespace TrickDetect;

public class EventSendAd { }

public class AdHandler
{
    public void SendAdToChat(EventSendAd _)
    {
        Server.NextFrame(() =>
        {
            Server.PrintToChatAll($" {ChatColors.Blue} Приветствуем вас воин!");
        });
    }
}
