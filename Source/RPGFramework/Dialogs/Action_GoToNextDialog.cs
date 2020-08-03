using UnityEngine;
using RimWorld;
using Verse;
using System.Linq;
using System.Text;
using Verse.AI;
using System.Collections.Generic;

namespace Quests
{
    public class Action_GoToNextDialog : DialogAction
    {
        public Action_GoToNextDialog()
        {

        }

        public override void DoAction()
        {
            var diaNode = new DiaNode(nextDialog.text);
            foreach (var option in nextDialog.options)
            {
                var diaOption = new DiaOption(option.text);
                diaOption.action = delegate ()
                {
                    foreach (var action in option.actions)
                    {
                        action.window = window;
                        action.DoAction();
                    }
                };
                diaNode.options.Add(diaOption);
            }
            window.GotoNode(diaNode);
        }
    }
}

