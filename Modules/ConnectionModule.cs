using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Logging;
using TrickDetect.Managers;

namespace TrickDetect.Modules;

public class ConnectionModule(PlayerManager playerManager, MapManager mapManager)
{
    public void OnPlayerConnect(EventOnPlayerConnect e)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                Map map = mapManager.GetAllMaps().First();
                var player = new Player(e.Slot, e.SteamId, e.Name, map);
                playerManager.AddPlayer(player);

                int userId = await playerManager.SelectOrInsertPlayerAsync(e.SteamId, e.Name);
                int points = await playerManager.SelectAllPlayerPointsAsync(userId);

                Server.NextFrame(() =>
                {
                    Server.PrintToChatAll($" {ChatColors.Magenta}Игрок {e.Name} {ChatColors.White} подключается к серверу | {ChatColors.Gold}Очков: {points}");
                });

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        });
    }

    public void OnPlayerDisconnect(EventOnPlayerDisconnect e)
    {
        if (e.SteamId == null)
            return;

        var client = Utilities.GetPlayerFromSlot(e.Slot);

        if (client != null)
        {
            playerManager.RemovePlayer(client);

            Server.NextFrame(() =>
            {
                Server.PrintToChatAll($" {ChatColors.Gold}Игрок {e.Name} {ChatColors.White} покидает сервер");
            });
        }
    }
}
