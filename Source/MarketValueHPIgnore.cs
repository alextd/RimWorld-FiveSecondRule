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
			//HitPoints only affects apparel/weapons,
			//so why does MarketValue care about HP if it has no effect?

			if (!def.useHitPoints || def.IsApparel || def.IsWeapon || def.IsCorpse)
				return;

			//1.0 will be SteadyAtmosphereEffects.FinalDeteriorationRate(__instance) == 0.0f
			if (__instance.GetSlotGroup()?.parent is Building_Storage || 
				__instance.Spawned && !SteadyAtmosphereEffects.InDeterioratingPosition(__instance))
			{
				__instance.HitPoints = __instance.MaxHitPoints;
			}
		}
	}
}