
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Utils;
using TrickDetect.Database;

namespace TrickDetect.Managers;

public class TrickManager(DB database)
{
  public class MapTricks
  {
    public Trick[] allTricks = [];
    public Dictionary<StartType, Dictionary<string, short>> tricksRoutesContain = new();
    public Dictionary<StartType, Dictionary<string, Trick>> tricksRoutesUnique = new();
  }

  private readonly Dictionary<Map, MapTricks> _tricks = new();

  public MapTricks GetTricksByMap(Map map)
  {
    return _tricks[map];
  }

  public void RouteChecker(Player player)
  {
    MapTricks mapTricks = GetTricksByMap(player.SelectedMap);

    if (string.IsNullOrEmpty(player.RouteTriggerPath))
    {
      player.ResetTrickProgress();
      return;
    }

    var containTricks = CheckRouteTrickMatching(mapTricks.allTricks, player, out Trick? matchingTrick);

    if (player.Debug)
    {
      player.Client.PrintToConsole($"> {player.RouteTriggerPath}");
      player.Client.PrintToConsole($"TotalContain > {containTricks}");
    }

    if (matchingTrick != null)
      CompleteTrick(player, matchingTrick);

    if (containTricks == 0)
    {
      player.StartType = StartType.Velocity;
      player.RouteTriggers.RemoveAt(0);
      RouteChecker(player);
    }
  }

  public int CheckRouteTrickMatching(Trick[] tricks, Player player, out Trick? matchingTrick)
  {
    int containTricks = 0;
    matchingTrick = null;

    foreach (var trick in tricks)
    {
      var trickRoute = trick.RouteTriggerPath;

      if ((trickRoute + ">").StartsWith(player.RouteTriggerPath + ">") && player.StartType == trick.StartType)
      {
        if (player.Debug)
          player.Client.PrintToConsole($"Contain > {trick.Name} | {trickRoute} | {trick.Point} {trick.StartType}");

        containTricks++;

        if (trickRoute == player.RouteTriggerPath)
          matchingTrick = trick;
      }
    }

    return containTricks;
  }

  // TODO
  // public int CheckRouteTrickMatching(MapTricks mapTricks, Player player, out Trick? matchingTrick)
  // {
  //   matchingTrick = null;
  //   short containTricks = 0;

  //   if (mapTricks.tricksRoutesContain.ContainsKey(player.StartType))
  //     if (mapTricks.tricksRoutesContain[player.StartType].ContainsKey(player.RouteTriggerPath))
  //       containTricks = mapTricks.tricksRoutesContain[player.StartType][player.RouteTriggerPath];

  //   if (containTricks > 0 && mapTricks.tricksRoutesUnique.ContainsKey(player.StartType))
  //     if (mapTricks.tricksRoutesUnique[player.StartType].ContainsKey(player.RouteTriggerPath))
  //       matchingTrick = mapTricks.tricksRoutesUnique[player.StartType][player.RouteTriggerPath];

  //   return containTricks;
  // }

  public async void CompleteTrick(Player player, Trick trick)
  {
    var totalAvgSpeed = CalculateTotalAvgSpeed(player);
    var totalTime = CalculateTotalTime(player);

    player.Client.PlayerPawn.Value!.HealthShotBoostExpirationTime = Server.CurrentTime + 1;
    Utilities.SetStateChanged(player.Client.PlayerPawn.Value, "CCSPlayerPawn", "m_flHealthShotBoostExpirationTime");
    player.Client.ExecuteClientCommand("play sounds\\weapons\\flashbang\\flashbang_explode1_distant.vsnd_c");

    var completeId = await InsertComplete(trick, player, totalAvgSpeed, totalTime);
    var wr = await SelectTrickWR(trick.Id);

    var trickMessage = BuildTrickMessage(player, trick);
    var timeMessage = BuildTimeMessage(totalTime, wr);
    var speedMessage = BuildSpeedMessage(totalAvgSpeed, wr);

    bool isWR = CheckForWR(wr, trick, totalAvgSpeed, totalTime, completeId);

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

  private int CalculateTotalAvgSpeed(Player player)
  {
    return (int)Math.Round(player.RouteTriggers.Sum(trigger => (trigger.AvgSpeedStartTouch ?? 0) + (trigger.AvgSpeedEndTouch ?? 0))) / player.RouteTriggers.Count;
  }

  private double CalculateTotalTime(Player player)
  {
    var firstTrigger = player.RouteTriggers.First();
    var lastTrigger = player.RouteTriggers.Last();
    return (lastTrigger.TimeStartTouch ?? 0) - (firstTrigger.TimeEndTouch ?? 0);
  }

  private string BuildTrickMessage(Player player, Trick trick)
  {
    return $"{ChatColors.Green}Trick {ChatColors.Grey}| {ChatColors.LightBlue}by {player.Info!.Name} {ChatColors.Gold}| Trick {trick.Name} {ChatColors.Green}| Points {ChatColors.Green}{trick.Point}";
  }

  private string BuildTimeMessage(double totalTime, TrickWR wr)
  {
    var timeMessage = $"{ChatColors.Green}Time {ChatColors.Grey}| {ChatColors.Orange}{Math.Round(totalTime, 2)}";
    if (wr.TimeWR == null || wr.TimeWR > totalTime)
    {
      timeMessage += $"{ChatColors.Purple} New record {ChatColors.Grey}|{ChatColors.LightYellow} {(wr.TimeWR ?? totalTime) - totalTime}";
    }
    else
    {
      timeMessage += $"{ChatColors.Purple} WR {ChatColors.DarkBlue}{Math.Round((float)wr.TimeWR, 2)} {ChatColors.Blue}by {wr.UsernameTimeWR}";
    }
    return timeMessage;
  }

  private string BuildSpeedMessage(int totalAvgSpeed, TrickWR wr)
  {
    var speedMessage = $"{ChatColors.Green}Спид {ChatColors.Grey}| {ChatColors.Orange}{totalAvgSpeed}";
    if (wr.SpeedWR == null || wr.SpeedWR < totalAvgSpeed)
    {
      speedMessage += $"{ChatColors.Purple} New record {ChatColors.Grey}|{ChatColors.LightYellow} +{totalAvgSpeed - (wr.SpeedWR ?? totalAvgSpeed)}";
    }
    else
    {
      speedMessage += $"{ChatColors.Purple} WR {ChatColors.DarkBlue}{wr.SpeedWR} {ChatColors.Blue}by {wr.UsernameSpeedWR}";
    }
    return speedMessage;
  }

  private bool CheckForWR(TrickWR wr, Trick trick, int totalAvgSpeed, double totalTime, int completeId)
  {
    bool isWR = false;
    if (wr.SpeedWR == null || wr.SpeedWR < totalAvgSpeed)
    {
      isWR = true;
      _ = database.ExecuteAsync(@"
            INSERT INTO public.""speed_wr"" (""completeId"", ""trickId"")
            VALUES (@completeId, @trickId) 
            ON CONFLICT (""trickId"") 
            DO UPDATE SET ""completeId"" = EXCLUDED.""completeId"";",
      new { completeId = completeId, trickId = trick.Id });
    }

    if (wr.TimeWR == null || wr.TimeWR > totalTime)
    {
      isWR = true;
      _ = database.ExecuteAsync(@"
            INSERT INTO public.""time_wr"" (""completeId"", ""trickId"")
            VALUES (@completeId, @trickId) 
            ON CONFLICT (""trickId"") 
            DO UPDATE SET ""completeId"" = EXCLUDED.""completeId"";",
      new { completeId = completeId, trickId = trick.Id });
    }
    return isWR;
  }

  // Api
  public async Task LoadAndSetMapTricks(Map map)
  {
    var tricks = await GetTricksFromDatabase(map);
    var triggers = await GetTriggersFromDatabase(map);

    var trickDict = tricks.ToDictionary(t => t.Id);
    foreach (var trigger in triggers)
    {
      if (trickDict.TryGetValue(trigger.TrickId, out var trick))
      {
        trick.Triggers ??= new List<Trigger>();
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

    var tricksRoutesContain = CalculateTricksRoutesContain(trickDict.Values);
    var tricksRoutesUnique = CalculateTricksRoutesUnique(trickDict.Values);

    _tricks[map] = new MapTricks
    {
      allTricks = trickDict.Values.ToArray(),
      tricksRoutesContain = tricksRoutesContain,
      tricksRoutesUnique = tricksRoutesUnique,
    };
  }
  private async Task<IEnumerable<Trick>> GetTricksFromDatabase(Map map)
  {
    return await database.QueryAsync<Trick>(@"
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
  }

  private async Task<IEnumerable<TriggersTrick>> GetTriggersFromDatabase(Map map)
  {
    return await database.QueryAsync<TriggersTrick>(@"
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
  }

  private Dictionary<StartType, Dictionary<string, short>> CalculateTricksRoutesContain(IEnumerable<Trick> tricks)
  {
    var tricksRoutesContain = new Dictionary<StartType, Dictionary<string, short>>();

    foreach (var trick in tricks)
    {
      var startType = trick.StartType;
      var route = trick.RouteTriggerPath;
      var triggersSplitted = route.Split(',');

      if (!tricksRoutesContain.ContainsKey(startType))
        tricksRoutesContain[startType] = new Dictionary<string, short>();

      var routesForStartType = tricksRoutesContain[startType];

      for (int i = 0; i < triggersSplitted.Length; i++)
      {
        for (int j = i + 1; j <= triggersSplitted.Length; j++)
        {
          var subRoute = string.Join(",", triggersSplitted.Skip(i).Take(j - i));
          if (!routesForStartType.ContainsKey(subRoute))
            routesForStartType[subRoute] = 0;

          routesForStartType[subRoute]++;
        }
      }
    }
    return tricksRoutesContain;
  }

  private Dictionary<StartType, Dictionary<string, Trick>> CalculateTricksRoutesUnique(IEnumerable<Trick> tricks)
  {
    var tricksRoutesUnique = new Dictionary<StartType, Dictionary<string, Trick>>();
    foreach (var trick in tricks)
    {
      var startType = trick.StartType;
      var route = trick.RouteTriggerPath;
      if (!tricksRoutesUnique.ContainsKey(startType))
        tricksRoutesUnique[startType] = new Dictionary<string, Trick>();

      tricksRoutesUnique[startType][route] = trick;
    }
    return tricksRoutesUnique;
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
