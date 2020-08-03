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
					Pawn pawn = list[i];
					if (pawn.RaceProps.Humanlike)
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

						ModMetaData modMetaData = ModLister.AllInstalledMods.FirstOrDefault((ModMetaData x) =>
							x != null && x.Name != null && x.Active && x.Name.StartsWith("Fallout Core"));
						string path = Path.GetFullPath(modMetaData.RootDir.ToString() + "/Presets/" + settlement.Faction.def.defName);
						DirectoryInfo directoryInfo = new DirectoryInfo(path);
						if (directoryInfo.Exists)
						{
							var file = directoryInfo.GetFiles().RandomElement();
							if (file != null)
							{
								Log.Message(file.FullName, true);
								if (orGenerateMap.GetComponent<MapComponentGeneration>().path.Length == 0)
								{
									orGenerateMap.GetComponent<MapComponentGeneration>().DoGeneration = true;
									orGenerateMap.GetComponent<MapComponentGeneration>().path = file.FullName;
								}
							}
						}
					}, "GeneratingMapForNewEncounter", false, null, true);
					return;
				}
				Map orGenerateMap2 = GetOrGenerateMapUtility.GetOrGenerateMap(settlement.Tile, null);
				CaravanEnterMapUtility.Enter(caravan, orGenerateMap2, CaravanEnterMode.Edge, 0, true, null);
			}
		}

		[HarmonyPatch(typeof(Pawn))]
		[HarmonyPatch("SpawnSetup")]
		public static class Patch_SpawnSetup
		{
			[HarmonyPostfix]
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

		[HarmonyPatch(typeof(SettlementUtility), "AttackNow")]
		public class GetOrGenerateMapPatch
		{
			[HarmonyPostfix]
			public static void Postfix(ref Caravan caravan, ref Settlement settlement)
			{
				ModMetaData modMetaData = ModLister.AllInstalledMods.FirstOrDefault((ModMetaData x) =>
												x != null && x.Name != null && x.Active && x.Name.StartsWith("Fallout Core"));
				string path = Path.GetFullPath(modMetaData.RootDir.ToString() + "/Presets/" + settlement.Faction.def.defName);
				DirectoryInfo directoryInfo = new DirectoryInfo(path);
				if (directoryInfo.Exists)
				{
					var file = directoryInfo.GetFiles().RandomElement();
					if (file != null)
					{
						Log.Message(file.FullName, true);
						if (settlement.Map.GetComponent<MapComponentGeneration>().path.Length == 0)
						{
							settlement.Map.GetComponent<MapComponentGeneration>().DoGeneration = true;
							settlement.Map.GetComponent<MapComponentGeneration>().path = file.FullName;
						}
					}
				}
			}
		}

		[HarmonyPatch(typeof(Settlement), "GetCaravanGizmos")]
		public class VisitSettlement
		{
			[HarmonyPostfix]
			public static void Postfix(Settlement __instance, ref IEnumerable<Gizmo> __result, Caravan caravan)
			{
				Command_Action command_Action = new Command_Action
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
							ModMetaData modMetaData = ModLister.AllInstalledMods.FirstOrDefault((ModMetaData x) =>
											x != null && x.Name != null && x.Active && x.Name.StartsWith("Fallout Core"));
							string path = Path.GetFullPath(modMetaData.RootDir.ToString() + "/Presets/" + __instance.Faction.def.defName);
							DirectoryInfo directoryInfo = new DirectoryInfo(path);
							if (directoryInfo.Exists)
							{
								var file = directoryInfo.GetFiles().RandomElement();
								if (file != null)
								{
									Log.Message(file.FullName, true);
									if (orGenerateMap.GetComponent<MapComponentGeneration>().path.Length == 0)
									{
										orGenerateMap.GetComponent<MapComponentGeneration>().DoGeneration = true;
										orGenerateMap.GetComponent<MapComponentGeneration>().path = file.FullName;
									}
								}
							}
						};
						LongEventHandler.QueueLongEvent(action, "GeneratingMapForNewEncounter", false, null, true);
					}
				};
				__result = CollectionExtensions.AddItem<Gizmo>(__result, command_Action);
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

