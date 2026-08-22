using CounterStrikeSharp.API.Core;
using TrickDetect.Database;

namespace TrickDetect.Managers;

public class PlayerManager(DB database)
{
  private readonly List<Player> _players = new();

  public void AddPlayer(Player player)
  {
    _players.RemoveAll(p => p.Slot == player.Slot);
    _players.Add(player);
  }

  public void RemovePlayer(int slot)
  {
    _players.RemoveAll(p => p.Slot == slot);
  }

  public void RemovePlayer(CCSPlayerController? client)
  {
    if (client == null) return;
    RemovePlayer(client.Slot);
  }

  public void RemovePlayer(Player? player)
  {
    if (player == null) return;
    RemovePlayer(player.Slot);
  }

  public Player? GetPlayer(CCSPlayerController? client)
  {
    if (client == null) return null;
    return _players.FirstOrDefault(p => p.Slot == client.Slot);
  }

  public Player? GetPlayer(int slot)
  {
    return _players.FirstOrDefault(p => p.Slot == slot);
  }

  public List<Player> GetPlayerList()
  {
    return _players;
  }

  // Api
  public async Task<int> SelectOrInsertPlayerAsync(string steamId, string name)
  {
    string query = @"
        INSERT INTO `user` (steamid, username) 
        VALUES (@steamid, @username) 
        ON DUPLICATE KEY UPDATE username = VALUES(username), id = LAST_INSERT_ID(id);
        SELECT LAST_INSERT_ID();
    ";
    var parameters = new { steamid = steamId, username = name };

    int userId = await database.QueryAsyncSingle<int>(query, parameters);

    return userId;
  }

  public async Task<int> SelectAllPlayerPointsAsync(int userId)
  {
    var points = await database.QueryAsync<int>(@"
      SELECT COALESCE(SUM(t.point), 0) AS points
      FROM (
          SELECT DISTINCT c.trickId AS id
          FROM `complete` c
          WHERE c.userId = @userId
      ) AS unique_tricks
      JOIN `trick` t ON unique_tricks.id = t.id;
      ",
      new { userId = userId }
    );

    return points.FirstOrDefault();
  }
}
