using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using HarmonyLib;
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
		public static void Restore(Thing thing, Map map = null)
		{
			ThingDef def = thing.def;
			if (map == null)
				map = thing.Map;
			//HitPoints only affects apparel/weapons,
			//so why does MarketValue care about HP if it has no effect?

			if (!def.CanEverDeteriorate || def == ThingDefOf.BurnedTree || def.IsApparel || def.IsWeapon || def.IsCorpse)
				return;

			//ignore everything that doesn't use StatPart_Health ,
			//if the thing isn't deteriorating, set back to full hp
			if ((map != null && thing.Position != null && thing.Position.GetSlotGroup(map)?.parent is Building_Storage) || 
				SteadyEnvironmentEffects.FinalDeteriorationRate(thing) == 0.0f)
			{
				if(thing.HitPoints != thing.MaxHitPoints) Log.Message($"5-sec Rule Restored {thing}");
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
				RestoreHPToSafeItem.Restore(__instance, map);
		}
	}

	/* This is another occurence of the "Mac/Linux crashes when patching a virtual, empty method"
	[HarmonyPatch(typeof(Thing))]
	[HarmonyPatch("PostMapInit")]
	public class PostMapInit_Patch
	{
		//public virtual void PostMapInit()
		public static void Postfix(Thing __instance)
		{
			RestoreHPToSafeItem.Restore(__instance);
		}
	}*/

	//Instead, patch where it is called:
	[HarmonyPatch(typeof(Map))]
	[HarmonyPatch("FinalizeInit")]
	public class Call_PostMapInit_Patch
	{
		//actually instead of transpiling the PostMapInit call, just do the same loop with this postfix
		public static void Postfix(Map __instance)
		{
			foreach (Thing current in __instance.listerThings.AllThings.ToList<Thing>())
				RestoreHPToSafeItem.Restore(current);
		}
	}


	[HarmonyPatch(typeof(Thing))]
	[HarmonyPatch("TryAbsorbStack")]
	public class TryAbsorbStack_Patch
	{
		//public virtual bool TryAbsorbStack(Thing other, bool respectStackLimit)
		public static void Postfix(Thing __instance)
		{
			RestoreHPToSafeItem.Restore(__instance);
		}
	}

	[HarmonyPatch(typeof(RoofGrid))]
	[HarmonyPatch("SetRoof")]
	public class SetRoof_Patch
	{
		//public void SetRoof(IntVec3 c, RoofDef def)
		public static void Postfix(RoofGrid __instance, IntVec3 c, RoofDef def, Map ___map)
		{
			if (def == null) return;

			Map map = ___map;

			if (map == null || map.thingGrid == null || !map.regionAndRoomUpdater.Enabled) return;

			foreach (Thing t in map.thingGrid.ThingsAt(c))
			{
				RestoreHPToSafeItem.Restore(t, map);
			}
		}
	}

	[HarmonyPatch(typeof(Room), "Notify_RoomShapeOrContainedBedsChanged")]
	public class NewRoom_Patch
	{
		//public void Notify_RoomShapeOrContainedBedsChanged()
		public static void Postfix(Room __instance)
		{
			if (__instance.Group != null && __instance.UsesOutdoorTemperature) return;
			Map map = __instance.Map;

			foreach (Thing t in __instance.ContainedAndAdjacentThings)
			{
				RestoreHPToSafeItem.Restore(t, map);
			}
		}
	}
}