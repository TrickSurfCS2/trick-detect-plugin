using CounterStrikeSharp.API.Core;
using System.Drawing;

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
	public static double GetSpeed(this CCSPlayerController? client)
	{
		if (client == null || !client.IsValid)
			return 0;

		var pawn = client.PlayerPawn?.Value;
		if (pawn == null || !pawn.IsValid || pawn.AbsVelocity == null)
			return 0;

		return Math.Round(pawn.AbsVelocity.Length2D());
	}

	public static void HideLegs(this CCSPlayerController? client)
	{
		if (client == null || !client.IsValid)
			return;

		var pawn = client.PlayerPawn?.Value;
		if (pawn == null || !pawn.IsValid)
			return;

		pawn.Render = Color.FromArgb(254, 254, 254, 254);
	}

	public static Location? GetLocation(this CCSPlayerController? client)
	{
		if (client == null || !client.IsValid)
			return null;

		var pawn = client.PlayerPawn?.Value;
		if (pawn == null || !pawn.IsValid)
			return null;

		var origin = new DimensionVector
		{
			X = pawn.AbsOrigin?.X ?? 0f,
			Y = pawn.AbsOrigin?.Y ?? 0f,
			Z = pawn.AbsOrigin?.Z ?? 0f
		};
		var angle = new DimensionVector
		{
			X = pawn.EyeAngles?.X ?? 0f,
			Y = pawn.EyeAngles?.Y ?? 0f,
			Z = 0f
		};
		var velocity = new DimensionVector
		{
			X = pawn.AbsVelocity?.X ?? 0f,
			Y = pawn.AbsVelocity?.Y ?? 0f,
			Z = pawn.AbsVelocity?.Z ?? 0f
		};

		return new Location
		{
			origin = origin,
			angle = angle,
			velocity = velocity,
		};
	}


	public static void StripWeapons(this CCSPlayerController client)
	{
		var weaponServices = client?.PlayerPawn?.Value?.WeaponServices;
		if (weaponServices == null) return;

		foreach (var weapon in weaponServices.MyWeapons)
		{
			if (!weapon.IsValid || weapon.Value == null || !weapon.Value.IsValid || !weapon.Value.DesignerName.Contains("weapon_"))
				continue;

			try
			{
				weapon.Value.Remove();
			}
			catch
			{
				continue;
			}
		}
	}
}
