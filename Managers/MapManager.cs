using TrickDetect.Database;

namespace TrickDetect.Managers;

public class MapManager(DB database)
{
  private readonly Map[] _maps = [];

  public Map[] GetAllMaps()
  {
    return _maps;
  }

  public Map GetMapByName(string name)
  {
    return _maps.FirstOrDefault(map => map.Name == name)!;
  }
  public Map GetMapById(int id)
  {
    return _maps.FirstOrDefault(map => map.Id == id)!;
  }

  // Api
  public async Task<Map[]> LoadAllMaps()
  {
    var maps = await database.QueryAsync<Map>(@"
      SELECT 
        id AS ""Id"",
        name AS ""Name"",
        ""fullName"" AS ""FullName"",
        preview AS ""PreviewImage"",
        ""createdAt"" AS ""CreatedAt"",
        ""updatedAt"" AS ""UpdatedAt""
      FROM public.map"
    );

    return maps.ToArray();
  }
}
