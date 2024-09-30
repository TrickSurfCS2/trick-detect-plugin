using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Utils;
using TrickDetect.Managers;

namespace TrickDetect.Modules;

public class ConnectionModule(PlayerManager playerManager, MapManager mapManager)
{
    public void OnPlayerConnect(EventOnPlayerConnect e)
    {

        Map map = mapManager.GetMapByName(TrickDetect._cfg!.DefaultMap);
        var player = new Player(e.Slot, map);
        playerManager.AddPlayer(player);

        _ = Task.Run(async () =>
        {
            try
            {
                int userId = await playerManager.SelectOrInsertPlayerAsync(e.SteamId, e.Name);
                player.Info = new Player.PlayerInfo
                {
                    Id = userId,
                    Name = e.Name,
                    SteamId = e.SteamId
                };

                // Not needed :D
                int points = await playerManager.SelectAllPlayerPointsAsync(userId);

                Server.NextFrame(() =>
                {
                    player.Client.PlayerPawn.Value!.Teleport(map.Origin.ToVector(), null, null);
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
