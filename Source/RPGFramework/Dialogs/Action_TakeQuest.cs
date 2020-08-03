using UnityEngine;
using RimWorld;
using Verse;
using System.Linq;
using System.Text;
using Verse.AI;
using System.Collections.Generic;

namespace Quests
{
    public class Action_TakeQuest : DialogAction
    {
        public Action_TakeQuest()
        {

        }

        public override void DoAction()
        {
            Quest quest = QuestUtility.GenerateQuestAndMakeAvailable(this.questDef, StorytellerUtility
                .DefaultThreatPointsNow(this.window.initiator.Map));
            Find.LetterStack.ReceiveLetter(quest.name, quest.description, 
                LetterDefOf.PositiveEvent, null, null, quest, null, null);
        }
    }
}

