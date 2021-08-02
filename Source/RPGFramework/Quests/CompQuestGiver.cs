using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using HarmonyLib;
using RimWorld;
using RimWorld.QuestGen;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Quests
{
	[StaticConstructorOnStartup]
	public class CompQuestGiver : ThingComp
	{
		public CompProperties_QuestGiver Props
		{
			get
			{
				return this.props as CompProperties_QuestGiver;
			}
		}
		public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
		{
			List<FloatMenuOption> list = base.CompFloatMenuOptions(selPawn).ToList<FloatMenuOption>();
			FloatMenuOption item = new FloatMenuOption(Translator.Translate("TalkToPawn"), delegate ()
			{
				Job job = new Job(QuestsDefOf.StartDialog);
				job.targetA = this.parent;
				job.playerForced = true;
				selPawn.jobs.TryTakeOrderedJob(job, 0);
			}, MenuOptionPriority.Default, null, null, 0f, null, null);
			list.Add(item);
			return list;
		}

        public void TestNode(QuestNode node, Pawn questGiver, QuestScriptDef script)
        {
            if (node is QuestNode_Sequence sequence)
            {
                foreach (var newNode in sequence.nodes.ListFullCopy())
                {
                    Log.Message(script + " - Node2: " + newNode);
                    TestNode(newNode, questGiver, script);
                }
            }
            else if (node is QuestNode_RandomNode randomNode)
            {
                foreach (var newNode in randomNode.nodes)
                {
                    Log.Message(script + " - Node2: " + newNode);
                    TestNode(newNode, questGiver, script);
                }
            }
        }

        public void GetNewRoot(QuestNode origRoot, ref QuestNode newRoot, Pawn questGiver, QuestScriptDef script)
        {
            if (origRoot is QuestNode_Sequence sequence1)
            {
                var newSequence = new QuestNode_Sequence();
                var newSequenceNode = (QuestNode)newSequence;
                if (newRoot is QuestNode_Sequence sequence5)
                {
                    sequence5.nodes.Add(newSequence);
                }
                else if (newRoot is QuestNode_RandomNode sequence6)
                {
                    sequence6.nodes.Add(newSequence);
                }
                foreach (var node in sequence1.nodes)
                {
                    GetNewRoot(node, ref newSequenceNode, questGiver, script);
                }
            }
            else if (origRoot is QuestNode_RandomNode sequence2)
            {
                var newSequence = new QuestNode_RandomNode();
                var newSequenceNode = (QuestNode)newSequence;
                if (newRoot is QuestNode_Sequence sequence5)
                {
                    sequence5.nodes.Add(newSequence);
                }
                else if (newRoot is QuestNode_RandomNode sequence6)
                {
                    sequence6.nodes.Add(newSequence);
                }
                foreach (var node in sequence2.nodes)
                {
                    GetNewRoot(node, ref newSequenceNode, questGiver, script);
                }
            }
            else if (origRoot is QuestNode_GetPawn)
            {
                var getPawn2 = new QuestNode_GetPawnForQuest();
                getPawn2.questGiver = questGiver;
                if (newRoot is QuestNode_Sequence sequence3)
                {
                    sequence3.nodes.Add(getPawn2);
                }
                else if (newRoot is QuestNode_RandomNode sequence4)
                {
                    sequence4.nodes.Add(getPawn2);
                }
            }
            else if (origRoot is QuestNode_GetFaction)
            {
                var getFaction2 = new QuestNode_GetFactionForQuest();
                getFaction2.questGiverFaction = questGiver.Faction;
                if (newRoot is QuestNode_Sequence sequence3)
                {
                    sequence3.nodes.Add(getFaction2);
                }
                else if (newRoot is QuestNode_RandomNode sequence4)
                {
                    sequence4.nodes.Add(getFaction2);
                }
            }
            else
            {
                if (newRoot is QuestNode_Sequence sequence3)
                {
                    sequence3.nodes.Add(origRoot);
                }
                else if (newRoot is QuestNode_RandomNode sequence4)
                {
                    sequence4.nodes.Add(origRoot);
                }
            }
        }
        public void GenerateQuest()
        {
            List<QuestScriptDef> list = new List<QuestScriptDef>();
            if (this.specificQuests != null && this.specificQuests.Count > 0)
            {
                list = this.specificQuests;
            }
            else
            {
                list = DefDatabase<QuestScriptDef>.AllDefs
                    .Where(x => !x.isRootSpecial && x.root is QuestNode_Sequence sequence
                    && sequence.nodes.Where(y => y is QuestNode_GetPawn getPawn
                    && getPawn.storeAs.ToString() == "asker" || y is QuestNode_GetFaction getFaction 
                    && (getFaction.storeAs.ToString() == "faction" 
                    || getFaction.storeAs.ToString() == "askerFaction")).Count() > 0).InRandomOrder().ToList();
            }
            var questGiver = this.parent as Pawn;
            Log.Message(" - GenerateQuest - foreach (var script in DefDatabase<QuestScriptDef>.AllDefs.Where" +
                "(x => x.root is QuestNode_Sequence sequence - 47", true);
            foreach (var script in list)
            {
                if (script.root is QuestNode_Sequence sequence)
                {
                    var newRoot = new QuestNode_Sequence();
                    var test2 = newRoot as QuestNode;
                    this.GetNewRoot(script.root, ref test2, questGiver, script);
                    var oldRoot = script.root;
                    //this.TestNode(script.root, questGiver, script);
                    var test3 = test2 as QuestNode_Sequence;
                    var test4 = test3.nodes.First();
                    //this.TestNode(test4, questGiver, script);
                    script.root = test4;
                    try
                    {
                        var slate = new Slate();
                        slate.Set<Pawn>("asker", questGiver, false);
                        slate.Set<Pawn>("joiner", questGiver, false);
                        slate.Set<Map>("map", this.parent.Map, false);
                        FloatRange value = StorytellerUtility.DefaultThreatPointsNow(this.parent.Map)
                              * new FloatRange(0.7f, 1.3f);
                        ThingSetMakerParams parms2 = default(ThingSetMakerParams);
                        parms2.totalMarketValueRange = new FloatRange?(value);
                        parms2.makingFaction = questGiver.Faction;
                        List<Thing> items = ThingSetMakerDefOf.Reward_ItemsStandard.root.Generate(parms2);
                        var TotalMarketValue = items.Sum(x => x.def.BaseMarketValue);
                        
                        slate.Set<List<Thing>>("itemsReward_items", items, false);
                        slate.Set<float>("itemsReward_totalMarketValue", TotalMarketValue, false);
                        slate.Set<IEnumerable<ThingDef>>("itemStashThings", items.Select(x => x.def), false);
                        var enemyFactions = Find.FactionManager.AllFactions.Where
                               (x => x != questGiver.Faction && 
                               x.RelationWith(questGiver.Faction).kind == FactionRelationKind.Hostile);
                        if (enemyFactions != null && enemyFactions.Count() > 0)
                        {
                            var enemyFaction = enemyFactions.RandomElement();
                            slate.Set<Faction>("enemyFaction", enemyFaction);
                        }
                        slate.Set<ThingDef>("itemStashSingleThing", ThingDefOf.AIPersonaCore, false);
                        slate.Set<ThingDef>("targetMineable", ThingDefOf.MineableGold, false);
                        slate.Set<Pawn>("worker", PawnsFinder.AllMaps_FreeColonists.FirstOrDefault<Pawn>(), false);
                        slate.Set<float>("points", StorytellerUtility.DefaultThreatPointsNow(this.parent.Map), false);
                        slate.Set<Faction>("faction", questGiver.Faction, false);
                        slate.Set<Faction>("askerFaction", questGiver.Faction, false);

                        Find.CurrentMap.StoryState.RecordRandomQuestFired(script);
                        if (script.IsRootDecree)
                        {
                            Pawn pawn = slate.Get<Pawn>("asker", null, false);
                            if (pawn.royalty.AllTitlesForReading.NullOrEmpty<RoyalTitle>())
                            {
                                pawn.royalty.SetTitle(Faction.OfEmpire, RoyalTitleDefOf.Knight, false, false, true);
                                Messages.Message("Dev: Gave " + RoyalTitleDefOf.Knight.label + " title to " + pawn.LabelCap, pawn, MessageTypeDefOf.NeutralEvent, false);
                            }
                        }
                        if (script.CanRun(slate))
                        {
                            var quest = QuestGen.Generate(script, slate);
                            Find.QuestManager.Add(quest);
                            Log.Message(script + " - " + quest.name + " - " + quest.description);
                            Find.LetterStack.ReceiveLetter(quest.name, quest.description, 
                                LetterDefOf.NeutralEvent, null, null, quest, null, null);
                            script.root = oldRoot;
                            //break;
                        }
                        else
                        {
                            Log.Message("Cant run " + script);
                            script.root = oldRoot;
                        }
                    }
                    catch { }
                }
            }
        }

        private static void RenderExclamationPointOverlay(Thing t)
		{
			if (t.Spawned)
			{
				Vector3 drawPos = t.DrawPos;
				drawPos.y = Altitudes.AltitudeFor(AltitudeLayer.MetaOverlays) + 0.28125f;
				if (t is Pawn)
				{
					drawPos.x += (float)t.def.size.x - 1f;
					drawPos.z += (float)t.def.size.z + 0.2f;
				}
				CompQuestGiver.RenderPulsingOverlayQuest(t, CompQuestGiver.QuestionMarkMat, drawPos, MeshPool.plane05);
			}
		}

		private static void RenderPulsingOverlayQuest(Thing thing, Material mat, Vector3 drawPos, Mesh mesh)
		{
			float num = ((float)Math.Sin((double)((Time.realtimeSinceStartup + 397f * (float)(thing.thingIDNumber % 571)) * 4f)) + 1f) * 0.5f;
			num = 0.3f + num * 0.7f;
			Material material = FadedMaterialPool.FadedVersionOf(mat, num);
			Graphics.DrawMesh(mesh, drawPos, Quaternion.identity, material, 0);
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
		}
		public override void PostDraw()
		{
			base.PostDraw();
			CompQuestGiver.RenderExclamationPointOverlay(this.parent);
		}

        public DialogDef startDialog;
		public List<QuestScriptDef> specificQuests = new List<QuestScriptDef>();
		private static readonly Material QuestionMarkMat = MaterialPool.MatFrom("UI/Overlays/QuestionMark", ShaderDatabase.MetaOverlay);
	}
}

