using CounterStrikeSharp.API.Core;
using Npgsql;
using TrickDetect.Database;

namespace TrickDetect.Managers;

public class PlayerManager(DB database)
{
  private readonly List<Player> _players = new();

  public void AddPlayer(Player player)
  {
    _players.Add(player);
  }

  public void RemovePlayer(CCSPlayerController client)
  {
    var playerToRemove = _players.FirstOrDefault(p => p.Client.Index == client.Index);
    if (playerToRemove != null)
    {

      _players.Remove(playerToRemove);
    }
  }

  public Player GetPlayer(CCSPlayerController client)
  {
    return _players.FirstOrDefault(p => p.Client.Index == client.Index)!;
  }

  public List<Player> GetPlayerList()
  {
    return _players;
  }

  // Api
  public async Task<int> GetOrInsertPlayerAsync(string steamId, string name)
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
      new("@steamid", steamId),
      new("@username", name)
    };

    int userId = await database.ExecuteAsync(query, parameters);

    return userId;
  }

  public async Task<int> GetAllPlayerPointsAsync(int userId)
  {
    var points = await database.QueryAsync<int>(@"
      SELECT SUM(t.""point"") AS points
      FROM (
          SELECT DISTINCT(c.""trickId"") as id
          FROM public.""complete"" c
          WHERE c.""userId"" = @userId
      ) AS unique_tricks
      JOIN trick t ON unique_tricks.""id"" = t.""id"";
      ",
      new { userId = userId }
    );

    return points.FirstOrDefault();
  }
}
