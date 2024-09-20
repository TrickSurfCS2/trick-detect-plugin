using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities;
using System.Text.RegularExpressions;

namespace TrickDetect;
internal class Helpers
{

	public static List<CCSPlayerController> GetPlayerFromName(string name)
	{
		return Utilities.GetPlayers().FindAll(x => x.PlayerName.Equals(name, StringComparison.OrdinalIgnoreCase));
	}

	public static List<CCSPlayerController> GetPlayerFromSteamid64(string steamid)
	{
		return GetValidPlayers().FindAll(x =>
			x.SteamID.ToString().Equals(steamid, StringComparison.OrdinalIgnoreCase)
		);
	}

	public static List<CCSPlayerController> GetPlayerFromIp(string ipAddress)
	{
		return GetValidPlayers().FindAll(x =>
			x.IpAddress != null &&
			x.IpAddress.Split(":")[0].Equals(ipAddress)
		);
	}

	public static List<CCSPlayerController> GetValidPlayers()
	{
		return Utilities.GetPlayers().FindAll(p => p is
		{ IsValid: true, IsBot: false, Connected: PlayerConnectedState.PlayerConnected });
	}

	public static IEnumerable<CCSPlayerController?> GetValidPlayersWithBots()
	{
		return Utilities.GetPlayers().FindAll(p =>
		 p is { IsValid: true, IsBot: false, IsHLTV: false } or { IsValid: true, IsBot: true, IsHLTV: false }
		);
	}

	public static bool IsValidSteamId64(string input)
	{
		const string pattern = @"^\d{17}$";
		return Regex.IsMatch(input, pattern);
	}

	public static bool ValidateSteamId(string input, out SteamID? steamId)
	{
		steamId = null;

		if (string.IsNullOrEmpty(input))
		{
			return false;
		}

		if (!SteamID.TryParse(input, out var parsedSteamId)) return false;

		steamId = parsedSteamId;
		return true;
	}

	public static bool IsValidIp(string input)
	{
		const string pattern = @"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";
		return Regex.IsMatch(input, pattern);
	}

	public static void KickPlayer(int userId, string? reason = null)
	{
		if (!string.IsNullOrEmpty(reason))
		{
			var escapeChars = reason.IndexOfAny([';', '|']);

			if (escapeChars != -1)
			{
				reason = reason[..escapeChars];
			}
		}

		Server.ExecuteCommand($"kickid {userId} {reason}");
	}

	public static void PrintToCenterAll(string message)
	{
		Utilities.GetPlayers().Where(p => p is { IsValid: true, IsBot: false, IsHLTV: false }).ToList().ForEach(client =>
		{
			client.PrintToCenter(message);
		});
	}

	private static string ConvertMinutesToTime(int minutes)
	{
		var time = TimeSpan.FromMinutes(minutes);

		return time.Days > 0 ? $"{time.Days}d {time.Hours}h {time.Minutes}m" : time.Hours > 0 ? $"{time.Hours}h {time.Minutes}m" : $"{time.Minutes}m";
	}

	public static bool ClientIsValidAndAlive(CCSPlayerController? client)
	{
		return client != null
			&& client.IsValid
			&& !client.IsBot
			&& client.UserId.HasValue
			&& client is { IsValid: true, IsBot: false, PawnIsAlive: true };
	}

	public static bool ClientIsValid(CCSPlayerController? client)
	{
		return client != null
			&& client.IsValid
			&& !client.IsBot
			&& client.UserId.HasValue
			&& client is { IsValid: true, IsBot: false };
	}
}
