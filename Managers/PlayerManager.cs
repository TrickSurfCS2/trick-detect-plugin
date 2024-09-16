
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace TrickDetect.Managers;

public class PlayerManager
{
  private readonly Dictionary<IntPtr, Player> _players = new();

  public void AddPlayer(Player player)
  {
    _players[player.Client.Handle] = player;
  }

  public void RemovePlayer(CCSPlayerController client)
  {
    _players.Remove(client.Handle);
  }

  public Player GetPlayer(CCSPlayerController client)
  {
    return _players[client.Handle];
  }

  public List<Player> GetPlayerList()
  {
    return _players.Values.ToList();
  }
}
