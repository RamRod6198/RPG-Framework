using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace Quests
{
	public class JobDriver_StartDialog : JobDriver
	{

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return ReservationUtility.Reserve(this.pawn, this.job.targetA, this.job, 1, -1, null, errorOnFailed);
		}

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
            yield return new Toil
            {
                defaultCompleteMode = ToilCompleteMode.Delay,
                initAction = delegate ()
                {
                    Log.Message(" - MakeNewToils - var comp = TargetA.Pawn.TryGetComp<CompQuestGiver>(); - 2", true);
                    var comp = TargetA.Pawn.TryGetComp<CompQuestGiver>();
                    Log.Message(" - MakeNewToils - if (comp != null) - 3", true);
                    if (comp != null)
                    {
                        Log.Message(" - MakeNewToils - var diaNode = new DiaNode(comp.startDialog.text); - 4", true);
                        Log.Message("comp.startDialog: " + comp.startDialog, true);
                        var diaNode = new DiaNode(comp.startDialog.text);
                        Log.Message(" - MakeNewToils - var window = new DialogWindow(diaNode); - 5", true);
                        var window = new DialogWindow(diaNode);
                        Log.Message(" - MakeNewToils - window.curDialog = comp.startDialog; - 6", true);
                        window.curDialog = comp.startDialog;
                        Log.Message(" - MakeNewToils - window.initiator = pawn; - 7", true);
                        window.initiator = pawn;
                        Log.Message(" - MakeNewToils - window.talker = comp.parent as Pawn; - 8", true);
                        window.talker = comp.parent as Pawn;
                        Log.Message(" - MakeNewToils - window.StartDialog(); - 9", true);
                        window.StartDialog();
                    }
                }
            };
            Log.Message(" - MakeNewToils - yield break; - 11", true);
            yield break;
        }
	}
}

