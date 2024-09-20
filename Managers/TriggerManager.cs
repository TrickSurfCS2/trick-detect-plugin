
using TrickDetect.Database;

namespace TrickDetect.Managers;

public class TriggerManager(DB database)
{
  private readonly Dictionary<Map, Trigger[]> _tiggers = new();

  public Trigger[] GetMapTriggers(Map map)
  {
    return _tiggers.GetValueOrDefault(map)!;
  }

  public void SetMapTriggers(Map map, Trigger[] triggers)
  {
    _tiggers.Add(map, triggers);
  }

  // Api
  public async Task<Trigger[]> LoadMapTriggers(Map map)
  {
    var triggers = await database.QueryAsync<Trigger>(@"
      SELECT 
        id AS ""Id"",
        name AS ""Name"",
        ""fullName"" AS ""FullName"",
        coords AS ""Origin"",
        preview AS ""PreviewImage"",
        ""createdAt"" AS ""CreatedAt"",
        ""updatedAt"" AS ""UpdatedAt""
      FROM public.trigger as t
      WHERE t.""mapId"" = @mapId;
      ",
      new { mapId = map.Id }
      );

    return triggers.ToArray();
  }
}
