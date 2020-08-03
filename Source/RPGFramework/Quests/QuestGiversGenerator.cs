using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Quests
{
	public static class HarmonyPatches
	{

		[HarmonyPatch(typeof(IncidentWorker_VisitorGroup), "TryConvertOnePawnToSmallTrader")]
		internal static class IncidentWorker_VisitorGroup_TryConvertOnePawnToSmallTrader
		{
			[HarmonyPostfix]
			private static void Postfix(ref List<Pawn> pawns)
			{
				Log.Message("Postfix 1");
				if (pawns.Count > 0 && Rand.Chance(0.8f))
				{
					var candidates = pawns.Where(x => x.def.race.Humanlike 
					&& x.AllComps.Where(y => y is CompQuestGiver).Count() == 0);
					if (candidates.Count() > 0)
					{
						Pawn pawn = candidates.RandomElement();
						var questComp = Current.Game.GetComponent<QuestTracker>();
						questComp.CreateQuestGiver(pawn);
					}
				}
			}
		}

		[HarmonyPatch(typeof(PawnGroupKindWorker_Trader))]
		[HarmonyPatch("GenerateTrader")]
		public class PawnGroupKindWorker_TraderPatch
		{
			[HarmonyPostfix]
			private static void Postfix(Pawn __result)
			{
				Log.Message("Postfix 2");
				if (Rand.Chance(0.8f) && __result != null && __result.def.race.Humanlike 
					&& __result.TryGetComp<CompQuestGiver>() == null)
				{
					var comp = new CompQuestGiver();
					var questComp = Current.Game.GetComponent<QuestTracker>();
					questComp.CreateQuestGiver(__result);
				}
			}
		}

		[HarmonyPatch(typeof(PawnGroupKindWorker_Trader))]
		[HarmonyPatch("GenerateGuards")]
		public class PawnGroupKindWorker_GuardPatch
		{
			[HarmonyPostfix]
			private static void Postfix(PawnGroupMakerParms parms, PawnGroupMaker groupMaker, Pawn trader, List<Thing> wares, ref List<Pawn> outPawns)
			{
				Log.Message("Postfix 3");
				if (Rand.Chance(0.8f) && outPawns.Count > 0)
				{
					var candidates = outPawns.Where(x => x.def.race.Humanlike 
					&& x.AllComps.Where(y => y is CompQuestGiver).Count() == 0);
					if (candidates.Count() > 0)
					{
						Pawn pawn = candidates.RandomElement();
						var questComp = Current.Game.GetComponent<QuestTracker>();
						questComp.CreateQuestGiver(pawn);
					}
				}
			}
		}
	}
}

