using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Timers;

namespace TrickDetect;

public partial class TrickDetect
{
  private void RegisterEvents()
  {
    // Listeners
    RegisterListener<Listeners.OnMapStart>(OnMapStart);
    RegisterListener<Listeners.OnGameServerSteamAPIActivated>(OnGameServerSteamAPIActivated);
    RegisterListener<Listeners.OnTick>(() => { _eventsManager.Publish(new EventOnTickEvent()); });

    // Events
    RegisterEventHandler<EventPlayerConnectFull>(OnEventPlayerConnectFull);
    RegisterEventHandler<EventRoundStart>(OnEventRoundStart);
    RegisterEventHandler<EventPlayerSpawn>(OnEventPlayerSpawn);
    RegisterEventHandler<EventPlayerDeath>(OnEventPlayerDeath);

    // Entity hooks
    HookEntityOutput("trigger_multiple", "OnStartTouch", HookOnStartTouch);
    HookEntityOutput("trigger_multiple", "OnEndTouch", HookOnEndTouch);
  }
  private void UnRegisterEvents()
  {
    DeregisterEventHandler<EventPlayerConnectFull>(OnEventPlayerConnectFull);
    DeregisterEventHandler<EventRoundStart>(OnEventRoundStart);
    DeregisterEventHandler<EventPlayerSpawn>(OnEventPlayerSpawn);
    DeregisterEventHandler<EventPlayerJump>(OnEventPlayerJump);
  }

  private void OnMapStart(string mapName)
  {

  }

  private void OnGameServerSteamAPIActivated()
  {

  }

  // Events
  private HookResult OnEventPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo info)
  {
    CCSPlayerController? client = @event.Userid;

    if (client == null || !client.IsValid || client.IsBot || !client.UserId.HasValue)
      return HookResult.Continue;

    var eventMsg = new EventOnPlayerConnect
    {
      Name = client.PlayerName,
      Slot = client.Slot,
      SteamId = client.SteamID.ToString()
    };

    _eventsManager.Publish(eventMsg);

    return HookResult.Continue;
  }
  private HookResult OnEventRoundStart(EventRoundStart @event, GameEventInfo info)
  {
    AddTimer(1.0f, () =>
     {
       Server.NextFrame(() =>
       {
         Server.ExecuteCommand("sv_cheats                1");
         Server.ExecuteCommand("mp_buytime               0");
         Server.ExecuteCommand("mp_roundtime             9999");
         Server.ExecuteCommand("sv_friction              4");
         Server.ExecuteCommand("sv_accelerate            10");
         Server.ExecuteCommand("sv_airaccelerate         9999");
         Server.ExecuteCommand("sv_maxvelocity           3500");
         Server.ExecuteCommand("sv_maxspeed              400");
         Server.ExecuteCommand("sv_wateraccelerate       2000");
         Server.ExecuteCommand("sv_stopspeed             100");
         Server.ExecuteCommand("sv_falldamage_scale      0");
         Server.ExecuteCommand("sv_enablebunnyhopping    true");
         Server.ExecuteCommand("sv_autobunnyhopping      true");
         Server.ExecuteCommand("sv_staminajumpcost       0");
         Server.ExecuteCommand("sv_staminalandcost       0");
         Server.ExecuteCommand("sv_staminarecoveryrate   0");
         Server.ExecuteCommand("sv_staminamax            0");
       });
     }, TimerFlags.STOP_ON_MAPCHANGE);

    return HookResult.Continue;
  }
  private HookResult OnEventPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
  {
    CCSPlayerController client = @event.Userid!;

    if (client == null || !client.IsValid || client.IsBot || !client.UserId.HasValue)
      return HookResult.Continue;

    client.PlayerPawn.Value!.Render = Color.FromArgb(254, client.PlayerPawn.Value.Render.R, client.PlayerPawn.Value.Render.G, client.PlayerPawn.Value.Render.B);

    var player = _playerManager.GetPlayer(client);
    player.ResetTrickProgress();

    return HookResult.Continue;
  }
  private HookResult OnEventPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
  {
    CCSPlayerController client = @event.Userid!;

    if (client == null || !client.IsValid || client.IsBot || !client.UserId.HasValue)
      return HookResult.Continue;

    var player = _playerManager.GetPlayer(client);
    player.ResetTrickProgress();

    return HookResult.Continue;
  }
  private HookResult OnEventPlayerJump(EventPlayerJump @event, GameEventInfo info)
  {
    CCSPlayerController? client = @event.Userid;

    if (client == null || !client.IsValid || client.IsBot || !client.UserId.HasValue)
      return HookResult.Continue;

    var player = _playerManager.GetPlayer(client);

    if (player.Jumps.Count > 2)
    {
      player.Client.PrintToChat(" \x07 Jump during trick...");
      player.ResetTrickProgress();
    }

    return HookResult.Continue;
  }

  // Entity hooks
  private HookResult HookOnStartTouch(CEntityIOOutput output, string name, CEntityInstance activator, CEntityInstance caller, CVariant value, float delay)
  {
    if (activator == null || caller == null || activator.DesignerName != "player")
    {
      return HookResult.Continue;
    }

    var pawn = new CCSPlayerPawn(activator.Handle).Controller.Value!.Handle;
    var client = new CCSPlayerController(pawn);

    var entName = caller.Entity?.Name;

    if (entName != null && Helpers.ClientIsValidAndAlive(client))
    {
      var player = _playerManager.GetPlayer(client);
      var triggerName = entName.ToString();
      var eventMsg = new EventOnStartTouchEvent
      {
        TriggerName = triggerName,
        Player = player
      };

      _eventsManager.Publish(eventMsg);
    }

    return HookResult.Continue;
  }

  private HookResult HookOnEndTouch(CEntityIOOutput output, string name, CEntityInstance activator, CEntityInstance caller, CVariant value, float delay)
  {
    if (activator == null || caller == null || activator.DesignerName != "player")
    {
      return HookResult.Continue;
    }

    var pawn = new CCSPlayerPawn(activator.Handle).Controller.Value!.Handle;
    var client = new CCSPlayerController(pawn);

    var entName = caller.Entity?.Name;

    if (entName != null && Helpers.ClientIsValidAndAlive(client))
    {
      var player = _playerManager.GetPlayer(client);
      var triggerName = entName.ToString();
      var eventMsg = new EventOnEndTouchEvent
      {
        TriggerName = triggerName,
        Player = player
      };

      _eventsManager.Publish(eventMsg);
    }

    return HookResult.Continue;
  }
}
