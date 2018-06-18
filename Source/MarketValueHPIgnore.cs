using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using RimWorld;
using Verse;


namespace Five_Second_Rule
{
	//[HarmonyPatch(typeof(StatPart_Health))]
	//[HarmonyPatch("AppliesTo")]
	//public class MarketValueHPApplies_Patch
	//{
	//	public static bool Prefix(ref bool __result, StatRequest req)
	//	{
	//		//return false if marketvalue and item works fine,
	//		//but StatPart doesn't know it's for MarketValue
	//		//would need to transpile inside statworker.finalizevalue
	//	}
	//}


	//Just set HP to full if roofed. It will make no difference to anything but value.
	//The rare occasion of a fire breaking out will not destroy as much, but that's too obscure to care about
	[HarmonyPatch(typeof(Thing))]
	[HarmonyPatch("SpawnSetup")]
	public class ThingSpawn_Patch
	{
		public static void Postfix(Thing __instance)
		{
			ThingDef def = __instance.def;

			//ignore everything that doesn't use StatPart_Health ,
			//if the thing isn't deteriorating, set back to full hp
			if (def.useHitPoints && !def.IsApparel && !def.IsCorpse && !def.IsWeapon
				&& __instance.Position.Roofed(__instance.Map)
				&& __instance.GetRoom(RegionType.Set_All) is Room room && !room.UsesOutdoorTemperature)
			{
				__instance.HitPoints = __instance.MaxHitPoints;
			}
		}
	}
}