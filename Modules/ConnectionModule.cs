using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Logging;
using Npgsql;
using TrickDetect.Database;
using TrickDetect.Managers;

namespace TrickDetect;

public class ConnectionModule(DB database, PlayerManager playerManager)
{
    private readonly DB _database = database;
    private readonly PlayerManager _playerManager = playerManager;

    public void OnPlayerConnect(EventOnPlayerConnect e)
    {
        TrickDetect._logger!.LogInformation("OnPlayerConnect");
        _ = Task.Run(async () =>
        {
            try
            {
                TrickDetect._logger!.LogInformation("1");
                string query = @"
                    INSERT INTO public.""user"" (steamid, username) 
                    VALUES (@steamid, @username) 
                    ON CONFLICT (steamid) 
                    DO UPDATE SET username = EXCLUDED.username
                    RETURNING id;
                ";
                var parameters = new NpgsqlParameter[]
                {
                    new("@steamid", e.SteamId),
                    new("@username", e.Name)
                };

                int userId = await _database.ExecuteAsync(query, parameters);

                int points = await _database.QueryAsync<int>(@"
                    SELECT SUM(t.""point"") AS points
                    FROM (
                        SELECT DISTINCT(c.""trickId"") as id
                        FROM public.""complete"" c
                        WHERE c.""userId"" = @userId
                    ) AS unique_tricks
                    JOIN trick t ON unique_tricks.""id"" = t.""id"";
                    ",
                    [new("@userId", userId)]
                );

                TrickDetect._logger!.LogInformation($"points {points}");

                var player = new Player(e.Slot, e.SteamId, e.Name);
                _playerManager.AddPlayer(player);

                Server.NextFrame(() =>
                {
                    Server.PrintToChatAll($" {ChatColors.Magenta}Игрок {e.Name} {ChatColors.Gold}{points} {ChatColors.White} подключается к серверу");
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
            _playerManager.RemovePlayer(client);

        Server.NextFrame(() =>
        {
            Server.PrintToChatAll($" {ChatColors.Gold}Игрок {e.Name} {ChatColors.White} покидает сервер");
        });
    }
}
