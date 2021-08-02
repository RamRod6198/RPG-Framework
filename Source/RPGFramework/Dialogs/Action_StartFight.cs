using UnityEngine;
using RimWorld;
using Verse;
using System.Linq;
using System.Text;
using Verse.AI;
using System.Collections.Generic;

namespace Quests
{
    public class Action_StartFight : DialogAction
    {
        public Action_StartFight()
        {

        }
        public override void DoAction()
        {
            this.window.talker.Faction.TryAffectGoodwillWith(Faction.OfPlayer, -20);
            Job job = JobMaker.MakeJob(JobDefOf.AttackMelee, this.window.initiator);
            this.window.talker.jobs.TryTakeOrderedJob(job);
        }
    }
}

