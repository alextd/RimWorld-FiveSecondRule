﻿using System;
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

			//Everything that doesn't use StatPart_Health ,
			//except MarketValue since why is it worth less if it's not affected by it?
			if (__instance.Position.Roofed(__instance.Map) && def.useHitPoints && !def.IsApparel && !def.IsCorpse && !def.IsWeapon)
			{
				__instance.HitPoints = __instance.MaxHitPoints;
			}
		}
	}
}