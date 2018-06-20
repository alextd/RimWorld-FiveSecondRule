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


	//Just set HP to full if roofed and roomed. It will make no difference to anything but value.
	//The rare occasion of a fire breaking out will not destroy as much, but that's too obscure to care about
	public class RestoreHPToSafeItem
	{
		public static void Restore(Thing thing)
		{
			ThingDef def = thing.def;

			//ignore everything that doesn't use StatPart_Health ,
			//if the thing isn't deteriorating, set back to full hp
			if (def.useHitPoints && !def.IsApparel && !def.IsCorpse && !def.IsWeapon
				&& thing.Position.Roofed(thing.Map)
				&& thing.GetRoom(RegionType.Set_All) is Room room && !room.UsesOutdoorTemperature)
			{
				thing.HitPoints = thing.MaxHitPoints;
			}
		}
	}

	[HarmonyPatch(typeof(Thing))]
	[HarmonyPatch("SpawnSetup")]
	public class ThingSpawn_Patch
	{
		//public override void SpawnSetup(Map map, bool respawningAfterLoad)
		public static void Postfix(Thing __instance, Map map, bool respawningAfterLoad)
		{
			if (map.regionAndRoomUpdater.Enabled)
				RestoreHPToSafeItem.Restore(__instance);
		}
	}

	[HarmonyPatch(typeof(Thing))]
	[HarmonyPatch("PostMapInit")]
	public class PostMapInit_Patch
	{
		//public virtual void PostMapInit()
		public static void Postfix(Thing __instance)
		{
			RestoreHPToSafeItem.Restore(__instance);
		}
	}
}