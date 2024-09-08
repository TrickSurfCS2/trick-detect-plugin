using CounterStrikeSharp.API.Core;

namespace TrickDetect;

public static class PlayerExtensions
{

	public static void Print(this CCSPlayerController client, string message = "")
	{
		// TODO Localize text
		client.PrintToChat(message.ToString());
	}

	public static string NativeSteamId3(this CCSPlayerController client)
	{
		var steamId64 = client.SteamID;
		var steamId32 = (steamId64 - 76561197960265728).ToString();
		var steamId3 = $"[U:1:{steamId32}]";

		return steamId3;
	}
}
