﻿using CounterStrikeSharp.API.Core;
using System.Text.Json.Serialization;

namespace TrickDetect.Config;

public class TrickDetectConfig : BasePluginConfig
{

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

}
