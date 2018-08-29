using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
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
		public static void Restore(Thing thing, Map map = null)
		{
			ThingDef def = thing.def;
			if (map == null)
				map = thing.Map;
			//HitPoints only affects apparel/weapons,
			//so why does MarketValue care about HP if it has no effect?

			if (!def.useHitPoints || def.IsApparel || def.IsWeapon || def.IsCorpse)
				return;

			//ignore everything that doesn't use StatPart_Health ,
			//if the thing isn't deteriorating, set back to full hp
			if (thing.Position.GetSlotGroup(map)?.parent is Building_Storage || 
				SteadyEnvironmentEffects.FinalDeteriorationRate(thing) == 0.0f)
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
				RestoreHPToSafeItem.Restore(__instance, map);
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
		public static void Postfix(RoofGrid __instance, IntVec3 c, RoofDef def)
		{
			if (def == null) return;
			Map map = (Map)AccessTools.Field(typeof(RoofGrid), "map").GetValue(__instance);
			if (map == null || map.thingGrid == null) return;
			foreach (Thing t in map.thingGrid.ThingsAt(c))
			{
				RestoreHPToSafeItem.Restore(t, map);
			}
		}
	}
}