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

<<<<<<< HEAD
		public void AddQuestGiver(Pawn pawn, List<QuestScriptDef> specificQuests = null, DialogDef startDialog = null)
=======
		public void AddQuestGiver(Pawn pawn, List<QuestScriptDef> specificQuests = null)
>>>>>>> 6765b49273e3a65219f60581fa18f517efd372de
		{
			var comp = new CompQuestGiver();
			comp.parent = pawn;
			comp.specificQuests = specificQuests;
<<<<<<< HEAD
			comp.startDialog = startDialog;
=======
>>>>>>> 6765b49273e3a65219f60581fa18f517efd372de
			Log.Message("Giving comp to " + pawn);
			pawn.AllComps.Add(comp);
		}

<<<<<<< HEAD
		public void CreateQuestGiver(Pawn pawn, List<QuestScriptDef> specificQuests = null, DialogDef startDialog = null)
		{
			this.AddQuestGiver(pawn, specificQuests, startDialog);
=======
		public void CreateQuestGiver(Pawn pawn, List<QuestScriptDef> specificQuests = null)
		{
			this.AddQuestGiver(pawn, specificQuests);
>>>>>>> 6765b49273e3a65219f60581fa18f517efd372de
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

