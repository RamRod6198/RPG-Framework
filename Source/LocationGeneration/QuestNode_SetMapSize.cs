using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using UnityEngine;
using Verse;
using Verse.AI;

namespace LocationGeneration
{
	public class QuestNode_SetMapSize : QuestNode
	{
		public SlateRef<int> tile;

		public SlateRef<IntVec3> mapSize;
		protected override bool TestRunInt(Slate slate)
		{
			return true;
		}
		protected override void RunInt()
		{
			Slate slate = QuestGen.slate;
			var worldComp = Find.World.GetComponent<WorldComponentGeneration>();
			worldComp.tileSizes[tile.GetValue(slate)] = mapSize.GetValue(slate);
		}
	}
}

