using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Quests
{
	public class QuestTracker : GameComponent
	{
		public Dictionary<Pawn, QuestConfig> questTracker;

		public List<Pawn> questKeys;

		public List<QuestConfig> questValues;

		public QuestTracker()
		{

		}

		public QuestTracker(Game game)
		{

		}
		public override void LoadedGame()
		{
			base.LoadedGame();
			if (questTracker != null)
			{
				foreach (var pawn in questTracker.Keys)
				{
					if (pawn.Spawned)
					{
						AddQuestGiver(pawn, questTracker[pawn].specificQuests);
					}
				}
			}
		}

		public void AddQuestGiver(Pawn pawn, List<QuestScriptDef> specificQuests = null)
		{
			var comp = new CompQuestGiver();
			comp.parent = pawn;
			comp.specificQuests = specificQuests;
			Log.Message("Giving comp to " + pawn);
			pawn.AllComps.Add(comp);
		}

		public void CreateQuestGiver(Pawn pawn, List<QuestScriptDef> specificQuests = null)
		{
			this.AddQuestGiver(pawn, specificQuests);
			var config = new QuestConfig();
			if (this.questTracker == null)
			{
				this.questTracker = new Dictionary<Pawn, QuestConfig>();
			}
			this.questTracker[pawn] = config;
		}
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Collections.Look<Pawn, QuestConfig>(ref this.questTracker, "questTracker"
				, LookMode.Reference, LookMode.Deep, ref questKeys, ref questValues);
		}
	}
}

