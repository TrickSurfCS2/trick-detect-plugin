using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Timers;

namespace TrickDetect;

public partial class TrickDetect
{
  private void RegisterEvents()
  {
    RegisterListener<Listeners.OnMapStart>(OnMapStart);
    RegisterListener<Listeners.OnGameServerSteamAPIActivated>(OnGameServerSteamAPIActivated);
    RegisterListener<Listeners.OnTick>(() => { _eventsManager.Publish(new EventOnTickEvent()); });

    // Events
    RegisterEventHandler<EventPlayerConnectFull>((@event, info) =>
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
    });

    RegisterEventHandler<EventRoundStart>((@event, info) =>
    {
      AddTimer(1.0f, () =>
      {
        Server.NextFrame(() =>
        {
          // Server.ExecuteCommand("mp_humanteam             CT");
          Server.ExecuteCommand("sv_cheats                1");
          Server.ExecuteCommand("mp_buytime               0");
          Server.ExecuteCommand("mp_roundtime             9999");
          Server.ExecuteCommand("sv_friction              4");
          Server.ExecuteCommand("sv_air_max_wishspeed     55");
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
    });

    // Entity hooks
    HookEntityOutput(
      "trigger_multiple",
      "OnStartTouch",
      (CEntityIOOutput output, string name, CEntityInstance activator, CEntityInstance caller, CVariant value, float delay) =>
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
        var triggerName = entName.ToString();
        var eventMsg = new EventOnStartTouchEvent
        {
          TriggerName = triggerName,
          PlayerSlot = client.Slot
        };

        _eventsManager.Publish(eventMsg);
      }

      return HookResult.Continue;
    });

    HookEntityOutput(
      "trigger_multiple",
      "OnEndTouch",
      (CEntityIOOutput output, string name, CEntityInstance activator, CEntityInstance caller, CVariant value, float delay) =>
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
        var triggerName = entName.ToString();
        var eventMsg = new EventOnEndTouchEvent
        {
          TriggerName = triggerName,
          PlayerSlot = client.Slot
        };

        _eventsManager.Publish(eventMsg);
      }

      return HookResult.Continue;
    });
  }

  private void OnMapStart(string mapName)
  {

  }

  private void OnGameServerSteamAPIActivated()
  {

  }
}
