using Dapper;
using TrickDetect.Database;

namespace TrickDetect.Managers;

public class MapManager(DB database)
{
  private Map[] _maps = [];

  public Map[] GetAllMaps()
  {
    return _maps;
  }

  public Map GetMapByName(string name)
  {
    var mapName = _maps.FirstOrDefault(map => map.Name == name)!;
    var mapFullName = _maps.FirstOrDefault(map => map.FullName == name)!;

    return mapName ?? mapFullName;
  }
  public Map GetMapById(int id)
  {
    return _maps.FirstOrDefault(map => map.Id == id)!;
  }

  // Api
  public async Task LoadAndSetAllMaps()
  {
    SqlMapper.AddTypeHandler(new DoubleArrayToFloatArrayMapper());

    var maps = await database.QueryAsync<Map>(@"
      SELECT 
        id AS ""Id"",
        name AS ""Name"",
        ""fullName"" AS ""FullName"",
        preview AS ""PreviewImage"",
        ""createdAt"" AS ""CreatedAt"",
        ""updatedAt"" AS ""UpdatedAt"",
        origin AS ""Origin""
      FROM public.""map""
    ");

    _maps = maps.ToArray();
  }
}
