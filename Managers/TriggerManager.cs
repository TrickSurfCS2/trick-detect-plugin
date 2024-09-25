
using CounterStrikeSharp.API;
using TrickDetect.Database;

namespace TrickDetect.Managers;

public class TriggerManager(DB database)
{
  private readonly Dictionary<Map, Trigger[]> _triggers = new();

  public Trigger[] GetTriggersByMap(Map map)
  {
    return _triggers.GetValueOrDefault(map)!;
  }

  public void SetMapTriggers(Map map, Trigger[] triggers)
  {
    _triggers.Add(map, triggers);
  }

  // Api
  public async Task LoadAndSetMapTriggers(Map map)
  {
    var triggers = await database.QueryAsync<Trigger>(@"
      SELECT 
        id AS ""Id"",
        name AS ""Name"",
        ""fullName"" AS ""FullName"",
        preview AS ""PreviewImage"",
        ""createdAt"" AS ""CreatedAt"",
        ""updatedAt"" AS ""UpdatedAt""
      FROM public.trigger as t
      WHERE t.""mapId"" = @mapId;
      ",
      new { mapId = map.Id }
      );

    _triggers[map] = triggers.ToArray();
  }
}
