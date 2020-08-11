using UnityEngine;
using RimWorld;
using Verse;
using System.Linq;
using System.Text;
using Verse.AI;
using System.Collections.Generic;

namespace Quests
{
    public class Action_StartTrade : DialogAction
    {
        public Action_StartTrade()
        {

        }

        public override void DoAction()
        {
            if (window.talker.trader != null && window.talker.trader.CanTradeNow)
            {
                Find.WindowStack.Add(new Dialog_Trade(this.window.initiator, window.talker, false));
            }
        }
    }
}

