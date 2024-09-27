
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Utils;
using TrickDetect.Database;

namespace TrickDetect.Managers;

public class TrickManager(DB database)
{
  private readonly Dictionary<Map, Trick[]> _tricks = new();

  public Trick[] GetTricksByMap(Map map)
  {
    return _tricks[map];
  }

  public void RouteChecker(Player player)
  {
    if (player.Debug)
      player.Client.PrintToConsole($"> {player.RouteTriggerPath}");

    int containTricks = 0;
    Trick[] tricks = GetTricksByMap(player.SelectedMap);

    foreach (var trick in tricks)
    {
      var trickRoute = trick.GetRouteTriggerPath();

      if (trickRoute.Contains(player.RouteTriggerPath) && player.StartType == trick.StartType)
      {
        if (player.Debug)
          player.Client.PrintToConsole($"CONTAIN > {trick.Name} | {trickRoute}");

        containTricks++;

        if (trickRoute == player.RouteTriggerPath)
          CompleteTrick(player, trick);
      }
    }

    if (player.Debug)
      player.Client.PrintToConsole($"TOTAL > {containTricks}");

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

  public async void CompleteTrick(Player player, Trick trick)
  {
    var totalAvgSpeed = (int)Math.Round(player.RouteTriggers.Sum(trigger => (trigger.AvgSpeedStartTouch ?? 0) + (trigger.AvgSpeedEndTouch ?? 0)));
    var firstTrigger = player.RouteTriggers.First();
    var lastTrigger = player.RouteTriggers.Last();
    var totalTime = (lastTrigger.TimeStartTouch ?? 0) - (firstTrigger.TimeEndTouch ?? 0);

    player.Client.PlayerPawn.Value!.HealthShotBoostExpirationTime = Server.CurrentTime + 1;
    Utilities.SetStateChanged(player.Client.PlayerPawn.Value, "CCSPlayerPawn", "m_flHealthShotBoostExpirationTime");
    player.Client.ExecuteClientCommand("play sounds\\weapons\\flashbang\\flashbang_explode1_distant.vsnd_c");

    var completeId = await InsertComplete(trick, player, totalAvgSpeed, totalTime);
    var wr = await SelectTrickWR(trick.Id);

    var trickMessage = $"{ChatColors.Green}Trick {ChatColors.Grey}| {ChatColors.LightBlue}by {player.Info!.Name} {ChatColors.Gold}| Trick {trick.Name} {ChatColors.Green}| Points {ChatColors.Green}{trick.Point}";
    var timeMessage = $"{ChatColors.Green}Time {ChatColors.Grey}| {ChatColors.Orange}{Math.Round(totalTime, 2)}";
    var speedMessage = $"{ChatColors.Green}Спид {ChatColors.Grey}| {ChatColors.Orange}{totalAvgSpeed}";
    var isWR = false;

    if (wr.SpeedWR == null || wr.SpeedWR < totalAvgSpeed)
    {
      speedMessage += $"{ChatColors.Purple} New record {ChatColors.Grey}|{ChatColors.LightYellow} +{totalAvgSpeed - (wr.SpeedWR ?? totalAvgSpeed)}";
      isWR = true;
      _ = database.ExecuteAsync(@"
        INSERT INTO public.""speed_wr"" (""completeId"", ""trickId"")
        VALUES (@completeId, @trickId) 
        ON CONFLICT (""trickId"") 
        DO UPDATE SET ""completeId"" = EXCLUDED.""completeId"";",
      new { completeId = completeId, trickId = trick.Id });
    }
    else
      speedMessage += $"{ChatColors.Purple} WR {ChatColors.DarkBlue}{wr.SpeedWR} {ChatColors.Blue}by {wr.UsernameSpeedWR}";

    if (wr.TimeWR == null || wr.TimeWR > totalTime)
    {
      timeMessage += $"{ChatColors.Purple} New record {ChatColors.Grey}|{ChatColors.LightYellow} {(wr.TimeWR ?? totalTime) - totalTime}";
      isWR = true;
      _ = database.ExecuteAsync(@"
        INSERT INTO public.""time_wr"" (""completeId"", ""trickId"")
        VALUES (@completeId, @trickId) 
        ON CONFLICT (""trickId"") 
        DO UPDATE SET ""completeId"" = EXCLUDED.""completeId"";",
      new { completeId = completeId, trickId = trick.Id });
    }
    else
      timeMessage += $"{ChatColors.Purple} WR {ChatColors.DarkBlue}{Math.Round((float)wr.TimeWR, 2)} {ChatColors.Blue}by {wr.UsernameTimeWR}";

    Server.NextFrame(() =>
    {
      if (player.Debug)
        player.Client.PrintToConsole($"CompleteId {completeId}");

      if (isWR)
        player.Client.ExecuteClientCommand("play sounds\\ambient\\ambient\\rainscapes\\thunder_close01.vsnd_c");

      player.Client.PrintToChat(trickMessage);
      player.Client.PrintToChat(timeMessage);
      player.Client.PrintToChat(speedMessage);
    });
  }

  // Api
  public async Task LoadAndSetMapTricks(Map map)
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
          CreatedAt = trigger.TriggerCreatedAt,
          UpdatedAt = trigger.TriggerUpdatedAt
        });
      }
    }

    _tricks[map] = trickDict.Values.ToArray();
  }

  public async Task<TrickWR> SelectTrickWR(int trickId)
  {
    var wr = await database.QueryAsync<TrickWR>(@"
        SELECT 
            twr.""time"" as ""TimeWR"",
            twr_user.""username"" as ""UsernameTimeWR"",
            swr.""speed"" as ""SpeedWR"",
            swr_user.""username"" as ""UsernameSpeedWR""
        FROM public.""trick"" as t
        LEFT JOIN 
            public.""complete"" twr ON twr.""id"" = 
                (SELECT twri.""completeId"" FROM public.""time_wr"" as twri WHERE twri.""trickId"" = @trickId)
        LEFT JOIN 
            public.""complete"" swr ON swr.""id"" = 
                (SELECT swri.""completeId"" FROM public.""speed_wr"" as swri WHERE swri.""trickId"" = @trickId)
        LEFT JOIN 
            public.""user"" twr_user ON twr.""userId"" = twr_user.""id""
        LEFT JOIN 
            public.""user"" swr_user ON swr.""userId"" = swr_user.""id""
        WHERE t.""id"" = @trickId;
      ",
      new { trickId = trickId }
    );

    return wr.First();
  }

  public async Task<int> InsertComplete(Trick trick, Player player, int speed, double time)
  {
    var completeId = await database.QueryAsyncSingle<int>(@"
      INSERT INTO ""complete""(""userId"", ""trickId"", speed, ""time"") 
      VALUES(@userId, @trickId, @speed, @time)
      RETURNING id;
    ",
      new { userId = player.Info!.Id, trickId = trick.Id, speed = speed, time = time }
    );

    return completeId;
  }
}
