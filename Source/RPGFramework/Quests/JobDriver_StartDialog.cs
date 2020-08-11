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
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
			yield return new Toil
			{
				defaultCompleteMode = ToilCompleteMode.Delay,
				initAction = delegate ()
				{
					//var comp = TargetA.Pawn.TryGetComp<CompQuestGiver>();
					//if (comp != null)
					//{
					//	comp.GenerateQuest();
					//	TargetA.Pawn.AllComps.Remove(comp);
					//}
				}
			};
			yield break;
		}
	}
}

