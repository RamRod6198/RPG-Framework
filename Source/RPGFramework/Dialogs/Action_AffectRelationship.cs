using UnityEngine;
using RimWorld;
using Verse;
using System.Linq;
using System.Text;
using Verse.AI;
using System.Collections.Generic;

namespace Quests
{
    public class Action_AffectRelationship : DialogAction
    {
        public Action_AffectRelationship()
        {

        }
        public override void DoAction()
        {
            this.window.talker.Faction.TryAffectGoodwillWith(Faction.OfPlayer, this.affectGoodwill);
        }
    }
}

