
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
    if (player.SelectedMap == null)
      return;

    MapTricks mapTricks = GetTricksByMap(player.SelectedMap);
    if (mapTricks?.allTricks == null)
      return;

    var client = player.Client;

    if (string.IsNullOrEmpty(player.RouteTriggerPath))
    {
      player.ResetTrickProgress();
      return;
    }

    var containTricks = CheckRouteTrickMatching(mapTricks.allTricks, player, out Trick? matchingTrick, out List<Trick> candidateTricks);

    if (player.DebugExtended)
    {
      var triggersList = player.RouteTriggers.Select(t => t.TouchedTrigger.Name).ToList();
      var pathStr = string.Join(" ➜ ", triggersList);

      client?.PrintToChat($" {ChatColors.Purple}[DETECTOR]{ChatColors.Grey} Path ({triggersList.Count}): {ChatColors.Green}{pathStr}");
      client?.PrintToChat($" {ChatColors.Purple}[DETECTOR]{ChatColors.Grey} StartType: {ChatColors.Yellow}{player.StartType}{ChatColors.Grey} | StartSpeed: {ChatColors.Yellow}{Math.Round(player.StartSpeed, 1)}{ChatColors.Grey} | Detect: {(containTricks > 0 ? $"{ChatColors.Green}ACTIVE ({containTricks} tricks)" : $"{ChatColors.Red}NO MATCHES")}");

      if (candidateTricks.Count > 0)
      {
        foreach (var candidate in candidateTricks.Take(3))
        {
          var candTriggers = candidate.Triggers.Select(t => t.Name).ToList();
          int currentStep = triggersList.Count;
          string nextTarget = currentStep < candTriggers.Count ? candTriggers[currentStep] : "🏁 [COMPLETE]";
          client?.PrintToChat($"  {ChatColors.Grey}• {ChatColors.Gold}{candidate.Name} {ChatColors.Grey}({currentStep}/{candTriggers.Count}) ➜ Next: {ChatColors.LightBlue}{nextTarget}");
        }
        if (candidateTricks.Count > 3)
        {
          client?.PrintToChat($"  {ChatColors.Grey}... and {candidateTricks.Count - 3} more trick(s)");
        }
      }
    }
    else if (player.Debug)
    {
      client?.PrintToConsole($"> {player.RouteTriggerPath}");
      client?.PrintToConsole($"TotalContain > {containTricks}");
      foreach (var cand in candidateTricks)
      {
        client?.PrintToConsole($"Contain > {cand.Name} | {cand.RouteTriggerPath} | {cand.Point} {cand.StartType}");
      }
    }

    if (matchingTrick != null)
    {
      if (player.DebugExtended)
      {
        client?.PrintToChat($" {ChatColors.Gold}★ [DETECTOR] MATCH COMPLETED: {ChatColors.Green}{matchingTrick.Name} ({matchingTrick.Point} pts)!{ChatColors.Gold} ★");
      }
      CompleteTrick(player, matchingTrick);
    }

    if (containTricks == 0)
    {
      if (player.DebugExtended)
      {
        client?.PrintToChat($" {ChatColors.Red}[DETECTOR] ✕ Route broken (0 matching tricks). Shifting window to Velocity...");
      }
      player.StartType = StartType.Velocity;
      player.RouteTriggers.RemoveAt(0);
      RouteChecker(player);
    }
  }

  public int CheckRouteTrickMatching(Trick[] tricks, Player player, out Trick? matchingTrick)
  {
    return CheckRouteTrickMatching(tricks, player, out matchingTrick, out _);
  }

  public int CheckRouteTrickMatching(Trick[] tricks, Player player, out Trick? matchingTrick, out List<Trick> candidateTricks)
  {
    int containTricks = 0;
    matchingTrick = null;
    candidateTricks = new List<Trick>();

    foreach (var trick in tricks)
    {
      var trickRoute = trick.RouteTriggerPath;
      if (string.IsNullOrEmpty(trickRoute))
        continue;

      if ((trickRoute + ",").StartsWith(player.RouteTriggerPath + ",") && player.StartType == trick.StartType)
      {
        candidateTricks.Add(trick);
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
    var totalAvgSpeed = CalculateTotalAvgSpeed(player, trick);
    var totalTime = CalculateTotalTime(player, trick);

    var pawn = player.Client?.PlayerPawn?.Value;
    if (pawn != null && pawn.IsValid)
    {
      pawn.HealthShotBoostExpirationTime = Server.CurrentTime + 1;
      Utilities.SetStateChanged(pawn, "CCSPlayerPawn", "m_flHealthShotBoostExpirationTime");
      player.Client?.ExecuteClientCommand("play sounds\\weapons\\flashbang\\flashbang_explode1_distant.vsnd_c");
    }

    var completeId = await InsertComplete(trick, player, totalAvgSpeed, totalTime);
    var wr = await SelectTrickWR(trick.Id);

    var trickMessage = BuildTrickMessage(player, trick);
    var timeMessage = BuildTimeMessage(totalTime, wr);
    var speedMessage = BuildSpeedMessage(totalAvgSpeed, wr);

    bool isWR = CheckForWR(wr, trick, totalAvgSpeed, totalTime, completeId);

    Server.NextFrame(() =>
    {
      if (player.Debug)
        player.Client?.PrintToConsole($"CompleteId {completeId}");

      if (isWR)
        player.Client?.ExecuteClientCommand("play sounds/ambient/ambience/rainscapes/thunder_close01.vsnd_c");

      player.Client?.PrintToChat(trickMessage);
      player.Client?.PrintToChat(timeMessage);
      player.Client?.PrintToChat(speedMessage);
    });
  }

  private int CalculateTotalAvgSpeed(Player player, Trick trick)
  {
    if (trick.StartType == StartType.Velocity)
    {
      var trigger = player.RouteTriggers.First()!;
      trigger.ProgressStartTouch = [];
    }

    double totalSpeed = 0;
    int count = 0;

    foreach (var routeTrigger in player.RouteTriggers)
    {
      if (routeTrigger.ProgressEndTouch != null)
      {
        foreach (var progress in routeTrigger.ProgressEndTouch)
        {
          totalSpeed += progress.Speed;
          count++;
        }
      }

      if (routeTrigger.ProgressStartTouch != null)
      {
        foreach (var progress in routeTrigger.ProgressStartTouch)
        {
          totalSpeed += progress.Speed;
          count++;
        }
      }
    }

    return (int)Math.Round(totalSpeed / count);
  }

  private double CalculateTotalTime(Player player, Trick trick)
  {
    var firstTrigger = player.RouteTriggers.First();
    var lastTrigger = player.RouteTriggers.Last();

    if (trick.StartType == StartType.Velocity)
      return ((float)lastTrigger.TimeStartTouch!) - ((float)firstTrigger.TimeStartTouch!);

    return ((float)lastTrigger.TimeStartTouch!) - ((float)firstTrigger.TimeEndTouch!);
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
            INSERT INTO `speed_wr` (`completeId`, `trickId`)
            VALUES (@completeId, @trickId) 
            ON DUPLICATE KEY UPDATE `completeId` = VALUES(`completeId`);",
      new { completeId = completeId, trickId = trick.Id });
    }

    if (wr.TimeWR == null || wr.TimeWR > totalTime)
    {
      isWR = true;
      _ = database.ExecuteAsync(@"
            INSERT INTO `time_wr` (`completeId`, `trickId`)
            VALUES (@completeId, @trickId) 
            ON DUPLICATE KEY UPDATE `completeId` = VALUES(`completeId`);",
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
            id AS `Id`,
            name AS `Name`,
            point AS `Point`,
            `authorId` AS `AuthorId`,
            `mapId` AS `MapId`,
            `startType` AS `StartType`,
            `createdAt` AS `CreatedAt`,
            `updatedAt` AS `UpdatedAt`
        FROM `trick` AS t
        WHERE t.`mapId` = @mapId;
        ",
        new { mapId = map.Id }
    );
  }

  private async Task<IEnumerable<TriggersTrick>> GetTriggersFromDatabase(Map map)
  {
    return await database.QueryAsync<TriggersTrick>(@"
        SELECT 
            t.id AS `TrickId`,
            trig.id AS `TriggerId`,
            trig.name AS `TriggerName`,
            trig.`fullName` AS `TriggerFullName`,
            trig.preview AS `TriggerPreview`,
            trig.`createdAt` AS `TriggerCreatedAt`,
            trig.`updatedAt` AS `TriggerUpdatedAt`
        FROM `trick` AS t
        JOIN `route` AS r ON t.id = r.`trickId`
        JOIN `trigger` AS trig ON r.`triggerId` = trig.id
        WHERE t.`mapId` = @mapId;
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
            twr.`time` AS `TimeWR`,
            twr_user.username AS `UsernameTimeWR`,
            swr.speed AS `SpeedWR`,
            swr_user.username AS `UsernameSpeedWR`
        FROM `trick` AS t
        LEFT JOIN 
            `complete` twr ON twr.id = 
                (SELECT twri.`completeId` FROM `time_wr` AS twri WHERE twri.`trickId` = @trickId LIMIT 1)
        LEFT JOIN 
            `complete` swr ON swr.id = 
                (SELECT swri.`completeId` FROM `speed_wr` AS swri WHERE swri.`trickId` = @trickId LIMIT 1)
        LEFT JOIN 
            `user` twr_user ON twr.`userId` = twr_user.id
        LEFT JOIN 
            `user` swr_user ON swr.`userId` = swr_user.id
        WHERE t.id = @trickId;
      ",
      new { trickId = trickId }
    );

    return wr.FirstOrDefault() ?? new TrickWR
    {
      TimeWR = null,
      SpeedWR = null,
      UsernameTimeWR = null,
      UsernameSpeedWR = null
    };
  }

  public async Task<int> InsertComplete(Trick trick, Player player, int speed, double time)
  {
    var completeId = await database.QueryAsyncSingle<int>(@"
      INSERT INTO `complete` (`userId`, `trickId`, speed, `time`) 
      VALUES (@userId, @trickId, @speed, @time);
      SELECT LAST_INSERT_ID();
    ",
      new { userId = player.Info!.Id, trickId = trick.Id, speed = speed, time = time }
    );

    return completeId;
  }
}
