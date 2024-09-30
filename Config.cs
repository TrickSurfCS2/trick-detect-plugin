using CounterStrikeSharp.API.Core;
using System.Text.Json.Serialization;
using Npgsql;
using TrickDetect.Database;
using Microsoft.Extensions.Logging;

namespace TrickDetect;

public partial class TrickDetect
{
  public TrickDetectConfig Config { get; set; } = new();

  public void OnConfigParsed(TrickDetectConfig config)
  {
    if (config.DatabaseHost.Length < 1 || config.DatabaseName.Length < 1 || config.DatabaseUser.Length < 1)
    {
      throw new Exception("[TrickDetect] You need to setup Database credentials in config!");
    }

    var builder = new NpgsqlConnectionStringBuilder
    {
      Host = config.DatabaseHost,
      Database = config.DatabaseName,
      Username = config.DatabaseUser,
      Password = config.DatabasePassword,
      Port = config.DatabasePort,
      Pooling = true,
      MinPoolSize = 0,
      MaxPoolSize = 640,
    };

    _database = new DB(builder.ConnectionString);

    if (!_database.CheckDatabaseConnection())
    {
      _logger!.LogError("Unable to connect to database!");
      Unload(false);
      return;
    }

    Config = config;
  }
}

public class TrickDetectConfig : BasePluginConfig
{

  // Database
  [JsonPropertyName("DatabaseHost")]
  public string DatabaseHost { get; set; } = "";

  [JsonPropertyName("DatabasePort")]
  public int DatabasePort { get; set; } = 5432;

  [JsonPropertyName("DatabaseUser")]
  public string DatabaseUser { get; set; } = "";

  [JsonPropertyName("DatabasePassword")]
  public string DatabasePassword { get; set; } = "";

  [JsonPropertyName("DatabaseName")]
  public string DatabaseName { get; set; } = "";

  [JsonPropertyName("PreSpeed")]
  public int PreSpeed { get; set; } = 400;

  [JsonPropertyName("DefaultMap")]
  public string DefaultMap { get; set; } = "ski2";
}
