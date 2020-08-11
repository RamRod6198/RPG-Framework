using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Quests
{
	public class QuestConfig : IExposable
	{
		public void ExposeData()
		{
			Scribe_Collections.Look<QuestScriptDef>(ref this.specificQuests, "specificQuests", LookMode.Def);
		}

		public List<QuestScriptDef> specificQuests = new List<QuestScriptDef>();

	}
}

