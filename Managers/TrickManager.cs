
using CounterStrikeSharp.API;
using TrickDetect.Database;

namespace TrickDetect.Managers;

public class TrickManager(DB database)
{
  private readonly Dictionary<Map, Trick[]> _tricks = new();

  public Trick[] SetTricksToMap(Map map, Trick[] tricks)
  {
    return _tricks[map] = tricks;
  }

  public Trick[] GetTrickByMap(Map map)
  {
    return _tricks[map];
  }

  public async Task<Trick[]> LoadMapTricks(Map map)
  {
    // Запрос для получения трюков
    var tricks = await database.QueryAsync<Trick>(@"
      SELECT 
        id as ""Id"",
        name as ""Name"",
        point as ""Point"",
        ""authorId"" as ""AuthorId"",
        ""mapId"" as ""MapId"",
        ""startType"" as ""StartType"",
        ""createdAt"" as ""CreatedAt"",
        ""updatedAt"" as ""UpdatedAt""
      FROM public.trick as t
      WHERE t.""mapId"" = @mapId;
      ",
      new { mapId = map.Id }
    );

    // Запрос для получения триггеров, связанных с трюками
    var triggers = await database.QueryAsync<TriggersTrick>(@"
      SELECT 
        t.""id"" as ""TrickId"",
        trig.""id"" as ""TriggerId"",
        trig.""name"" as ""TriggerName"",
        trig.""fullName"" as ""TriggerFullName"",
        trig.""preview"" as ""TriggerPreview"",
        trig.""coords"" as ""TriggerCoords"",
        trig.""createdAt"" as ""TriggerCreatedAt"",
        trig.""updatedAt"" as ""TriggerUpdatedAt""
      FROM public.""trick"" as t
      JOIN public.""route"" as r ON t.""id"" = r.""trickId""
      JOIN public.""trigger"" as trig ON r.""triggerId"" = trig.""id""
      WHERE t.""mapId"" = @mapId;
      ",
      new { mapId = map.Id }
    );

    // Объединение данных
    var trickDict = tricks.ToDictionary(t => t.Id);
    foreach (var trigger in triggers)
    {
      var trickId = trigger.TrickId;
      if (trickDict.TryGetValue(trickId, out var trick))
      {
        if (trick.Triggers == null)
          trick.Triggers = [];

        trick.Triggers.Add(new Trigger
        {
          Id = trigger.TriggerId,
          Name = trigger.TriggerName,
          FullName = trigger.TriggerFullName,
          PreviewImage = trigger.TriggerPreview,
          Origin = trigger.TriggerCoords,
          CreatedAt = trigger.TriggerCreatedAt,
          UpdatedAt = trigger.TriggerUpdatedAt
        });
      }
    }

    return trickDict.Values.ToArray();
  }


  public void RouteChecker(Player player)
  {
    if (player.Debug)
    {
      player.Client.PrintToConsole(" ");
      player.Client.PrintToConsole($"TRIGGERS NOW >> {player.RouteTriggerPath}");
      player.Client.PrintToConsole(" ");
    }

    int containTricks = 0;
    Trick[] tricks = GetTrickByMap(player.SelectedMap);

    foreach (var trick in tricks)
    {
      var trickRoute = trick.GetRouteTriggerPath();
      if (trickRoute.Contains(player.RouteTriggerPath) && player.StartType == trick.StartType)
      {
        if (player.Debug)
          player.Client.PrintToConsole($"CONTAIN >> {trick.Name} | {trickRoute}");

        containTricks++;

        if (trickRoute == player.RouteTriggerPath)
          CompleteTrick(player, trick);
      }
    }

    if (player.Debug)
    {
      player.Client.PrintToConsole(" ");
      player.Client.PrintToConsole($"TOTAL CONTAIN >> {containTricks}");
      player.Client.PrintToConsole(" ");
    }

    if (string.IsNullOrEmpty(player.RouteTriggerPath))
    {
      player.ResetTrickProgress();
      return;
    }

    if (containTricks == 0)
    {
      player.StartType = StartType.Velocity;
      player.RouteTriggers.RemoveAt(player.RouteTriggers.Count - 1);
      RouteChecker(player);
    }
  }

  public void CompleteTrick(Player player, Trick trick)
  {
    player.Client.PlayerPawn.Value!.HealthShotBoostExpirationTime = Server.CurrentTime + 1;
    Utilities.SetStateChanged(player.Client.PlayerPawn.Value, "CCSPlayerPawn", "m_flHealthShotBoostExpirationTime");
    player.Client.ExecuteClientCommand($"play sounds\\weapons\\flashbang\\flashbang_explode1_distant.vsnd_c");
    player.Client.PrintToChat($"Complete trick {trick.Name}");
  }
}
