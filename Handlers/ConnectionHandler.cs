using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Utils;
using Npgsql;
using TrickDetect.Database;
using Microsoft.Extensions.Logging;

namespace TrickDetect;

public class ConnectionHandler(DB database)
{
    private readonly DB database = database;

    public void OnPlayerConnect(EventOnPlayerConnect e)
    {
        _ = Task.Run(async () =>
        {
            try
            {
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
                    new("@username",  e.Name)
                };

                int userId = await database.ExecuteAsync(query, parameters);

                int points = await database.QueryAsync<int>(@"
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

        Server.NextFrame(() =>
        {
            Server.PrintToChatAll($" {ChatColors.Gold}Игрок {e.Name} {ChatColors.White} покидает сервер");
        });
    }
}
