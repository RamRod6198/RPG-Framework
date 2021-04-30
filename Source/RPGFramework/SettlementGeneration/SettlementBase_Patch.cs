using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Quests
{
	internal class SettlementBase_Patch
	{
		[HarmonyPatch(typeof(SettlementDefeatUtility))]
		[HarmonyPatch("CheckDefeated")]
		public static class Patch_SettlementDefeatUtility_IsDefeated
		{
			private static bool IsDefeated(Map map, Faction faction)
			{
				List<Pawn> list = map.mapPawns.SpawnedPawnsInFaction(faction);
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].RaceProps.Humanlike)
					{
						return false;
					}
				}
				return true;
			}
			[HarmonyPrefix]
			private static bool Prefix(Settlement factionBase)
			{
				bool result;
				if (factionBase.HasMap)
				{
					if (!IsDefeated(factionBase.Map, factionBase.Faction))
					{
						result = false;
					}
					else
					{
						result = true;
					}
				}
				else
				{
					result = true;
				}
				return result;
			}
		}

		[HarmonyPatch(typeof(CaravanArrivalAction_VisitSettlement))]
		[HarmonyPatch("Arrived")]
		public static class CaravanVisitPatch
		{
			[HarmonyPostfix]
			public static void Postfix(CaravanArrivalAction_VisitSettlement __instance, Caravan caravan)
			{
				Settlement settlement = Traverse.Create(__instance).Field("settlement").GetValue<Settlement>();
				if (!settlement.HasMap)
				{
					LongEventHandler.QueueLongEvent(delegate ()
					{
						Map orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(settlement.Tile, null);
						CaravanEnterMapUtility.Enter(caravan, orGenerateMap, CaravanEnterMode.Edge, 0, true, null);
						SettlementGeneration.InitialiseSettlementGeneration(orGenerateMap, settlement);
					}, "GeneratingMapForNewEncounter", false, null, true);
					return;
				}
				Map orGenerateMap2 = GetOrGenerateMapUtility.GetOrGenerateMap(settlement.Tile, null);
				CaravanEnterMapUtility.Enter(caravan, orGenerateMap2, CaravanEnterMode.Edge, 0, true, null);
			}
		}

		[HarmonyPatch(typeof(SettlementUtility), "AttackNow")]
		public class GetOrGenerateMapPatch
		{
			[HarmonyPostfix]
			public static void Postfix(ref Caravan caravan, ref Settlement settlement)
			{
				SettlementGeneration.InitialiseSettlementGeneration(settlement.Map, settlement);
			}
		}

		[HarmonyPatch(typeof(Settlement), "GetCaravanGizmos")]
		public class VisitSettlement
		{
			public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, Settlement __instance, Caravan caravan)
			{
				foreach (var g in __result)
                {
					yield return g;
                }
				yield return new Command_Action
				{
					icon = SettleUtility.SettleCommandTex,
					defaultLabel = Translator.Translate("VisitSettlement"),
					defaultDesc = Translator.Translate("VisitSettlementDesc"),
					action = delegate ()
					{
						Action action = delegate ()
						{
							Map orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(__instance.Tile, null);
							CaravanEnterMapUtility.Enter(caravan, orGenerateMap, CaravanEnterMode.Edge, 0, true, null);
							SettlementGeneration.InitialiseSettlementGeneration(orGenerateMap, __instance);
						};
						LongEventHandler.QueueLongEvent(action, "GeneratingMapForNewEncounter", false, null, true);
					}
				};
			}
		}

		[HarmonyPatch(typeof(Pawn))]
		[HarmonyPatch("SpawnSetup")]
		public static class Patch_SpawnSetup
		{
			public static void Postfix(Thing __instance)
			{
				if (__instance is Pawn pawn && pawn.RaceProps.Humanlike)
				{
					try
					{
						var questComp = Current.Game.GetComponent<QuestTracker>();
						if (questComp.questTracker.ContainsKey(pawn) && pawn.TryGetComp<CompQuestGiver>() == null)
						{
							var comp = new CompQuestGiver();
							comp.Initialize(new CompProperties_QuestGiver());
							comp.parent = pawn;
							comp.specificQuests = questComp.questTracker[pawn].specificQuests;
							if (!pawn.Dead)
							{
								pawn.AllComps.Add(comp);
							}
						}
					}
					catch { };
				}
			}
		}

		//[HarmonyPatch(typeof(Site), "GetCaravanGizmos")]
		//public class VisitSite
		//{
		//	[HarmonyPostfix]
		//	public static void Postfix(Site __instance, ref IEnumerable<Gizmo> __result, Caravan caravan)
		//	{
		//		Command_Action command_Action = new Command_Action
		//		{
		//			icon = SettleUtility.SettleCommandTex,
		//			defaultLabel = Translator.Translate("VisitSite"),
		//			defaultDesc = Translator.Translate("VisitSiteDesc"),
		//			action = delegate ()
		//			{
		//				Action action = delegate ()
		//				{
		//					Map orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(__instance.Tile, null);
		//					CaravanEnterMapUtility.Enter(caravan, orGenerateMap, CaravanEnterMode.Edge, 0, true, null);
		//				};
		//				LongEventHandler.QueueLongEvent(action, "GeneratingMapForNewEncounter", false, null, true);
		//			}
		//		};
		//		__result = CollectionExtensions.AddItem<Gizmo>(__result, command_Action);
		//	}
		//}
	}

	//internal class SettlementBase_FloatPatch
	//{
	//	[HarmonyPatch(typeof(Settlement), "get_Visitable")]
	//	public class VisitSettlementFloat
	//	{
	//		[HarmonyPostfix]
	//		public static void Postfix(Settlement __instance, ref bool __result)
	//		{
	//			//List<FloatMenuOption> list = __result.ToList<FloatMenuOption>();
	//			//CaravanArrivalAction_VisitSettlement test = 
	//			//foreach (FloatMenuOption floatMenuOption in CaravanArrivalAction_VisitSettlement
	//			//	.GetFloatMenuOptions(caravan, __instance))
	//			//{
	//			//	list.Add(floatMenuOption);
	//			//}
	//			__result = true;
	//		}
	//	}
	//
	//	[HarmonyPatch(typeof(Site), "get_Visitable")]
	//	public class VisitSiteFloat
	//	{
	//		[HarmonyPostfix]
	//		public static void Postfix(Site __instance, ref bool __result)
	//		{
	//			//List<FloatMenuOption> list = __result.ToList<FloatMenuOption>();
	//			//CaravanArrivalAction_VisitSettlement test = 
	//			//foreach (FloatMenuOption floatMenuOption in CaravanArrivalAction_VisitSettlement
	//			//	.GetFloatMenuOptions(caravan, __instance))
	//			//{
	//			//	list.Add(floatMenuOption);
	//			//}
	//			__result = true;
	//		}
	//	}
	//}
}

