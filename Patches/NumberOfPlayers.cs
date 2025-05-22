using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Kitchen.NetworkSupport;
using Steamworks;
using Steamworks.Data;
using Kitchen;
using KitchenData;
using Kitchen.Modules;
using UnityEngine;

namespace KitchenApplianceShop.Patches
{
	[HarmonyPatch(typeof(PlayerInfoManager), "UpdateDisplay")]
	public class UpdateDisplayOverridePatch_UpdateDisplay
	{
		[HarmonyPrefix]
		public static bool Prefix(PlayerInfoManager __instance)
		{
			Traverse PIM = Traverse.Create(__instance);

			List<PlayerInfo> list = PIM.Property("Players").GetValue<IPlayers>().All();
			Main.numberOfPlayers = list.Count;
			return false;
		}
	}
}