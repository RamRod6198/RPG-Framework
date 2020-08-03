using UnityEngine;
using RimWorld;
using Verse;
using System.Linq;
using System.Text;
using Verse.AI;
using System.Collections.Generic;

namespace Quests
{
    public class Action_EndDialog : DialogAction
    {
        public Action_EndDialog()
        {

        }

        public override void DoAction()
        {
            this.window.Close();
        }
    }
}

