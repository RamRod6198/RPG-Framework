using UnityEngine;
using RimWorld;
using Verse;
using System.Linq;
using System.Text;
using Verse.AI;
using System.Collections.Generic;

namespace Quests
{
    public class DialogAction
    {
        public virtual void DoAction()
        {

        }

        public QuestScriptDef questDef;

        public string initiator;

        public DialogWindow window;

        public DialogDef nextDialog;

        public int affectGoodwill;
    }
}

