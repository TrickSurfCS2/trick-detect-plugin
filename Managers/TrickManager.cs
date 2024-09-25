
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
    {
      player.Client.PrintToConsole(" ");
      player.Client.PrintToConsole($">>> {player.RouteTriggerPath}");
    }

    int containTricks = 0;
    Trick[] tricks = GetTricksByMap(player.SelectedMap);

    foreach (var trick in tricks)
    {
      var trickRoute = trick.GetRouteTriggerPath();

      if (trickRoute.Contains(player.RouteTriggerPath) && player.StartType == trick.StartType)
      {
        if (player.Debug)
          player.Client.PrintToConsole($"CONTAIN >>> {trick.Name} | {trickRoute}");

        containTricks++;

        if (trickRoute == player.RouteTriggerPath)
          CompleteTrick(player, trick);
      }
    }

    if (player.Debug)
    {
      player.Client.PrintToConsole($"TOTAL >>> {containTricks}");
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
    double totalAvgSpeed = 0.0;
    double totalTime = 0.0;

    // Суммируем все AvgSpeedStartTouch и AvgSpeedEndTouch
    foreach (var trigger in player.RouteTriggers)
    {
      if (trigger.AvgSpeedStartTouch.HasValue)
        totalAvgSpeed += trigger.AvgSpeedStartTouch.Value;
      if (trigger.AvgSpeedEndTouch.HasValue)
        totalAvgSpeed += trigger.AvgSpeedEndTouch.Value;
    }

    // Вычисляем общее количество значений скорости
    int speedCount = player.RouteTriggers.Count * 2; // Каждый триггер имеет два значения скорости

    // Вычисляем среднюю скорость
    totalAvgSpeed /= speedCount;

    // Вычисляем время от первого касания до последнего
    var firstTrigger = player.RouteTriggers.First();
    var lastTrigger = player.RouteTriggers.Last();

    if (firstTrigger.TimeEndTouch.HasValue && lastTrigger.TimeStartTouch.HasValue)
      totalTime = lastTrigger.TimeStartTouch.Value - firstTrigger.TimeEndTouch.Value;


    player.Client.PlayerPawn.Value!.HealthShotBoostExpirationTime = Server.CurrentTime + 1;
    Utilities.SetStateChanged(player.Client.PlayerPawn.Value, "CCSPlayerPawn", "m_flHealthShotBoostExpirationTime");
    player.Client.ExecuteClientCommand($"play sounds\\weapons\\flashbang\\flashbang_explode1_distant.vsnd_c");
    player.Client.PrintToChat($"Complete trick {trick.Name} | {totalAvgSpeed} _ {totalTime}");

    var WorldRecordTime = "0.0";
    var WorldRecordSpeed = "0.0";
    var WorldRecordTimeHolder = "Unnamed";
    var WorldRecordSpeedHolder = "Unnamed";

    string trickMessage = $"{ChatColors.Green}Trick {ChatColors.Grey}| {ChatColors.LightBlue}by {player.Info.Name} {ChatColors.Yellow}| Trick {trick.Name} {ChatColors.Green}| Points {ChatColors.Green}{trick.Point}";
    string timeMessage = $"{ChatColors.Green}Time {ChatColors.Grey}| {ChatColors.Yellow}{Math.Round(totalTime, 2)} {ChatColors.Grey}WR {ChatColors.DarkBlue}{WorldRecordTime} {ChatColors.Blue}by {WorldRecordTimeHolder}";
    string speedMessage = $"{ChatColors.Green}Спид {ChatColors.Grey}| {ChatColors.Yellow}{Math.Round(totalAvgSpeed)} {ChatColors.Grey}WR {ChatColors.DarkBlue}{WorldRecordSpeed} {ChatColors.Blue}by {WorldRecordSpeedHolder}";

    player.Client.PrintToChat(trickMessage);
    player.Client.PrintToChat(timeMessage);
    player.Client.PrintToChat(speedMessage);
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
}
