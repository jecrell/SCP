// ----------------------------------------------------------------------
// These are basic usings. Always let them be here.
// ----------------------------------------------------------------------
using System.Collections.Generic;
using System.Diagnostics;

// ----------------------------------------------------------------------
// These are RimWorld-specific usings. Activate/Deactivate what you need:
// ----------------------------------------------------------------------
//using VerseBase;         // Material/Graphics handling functions are found here
using Verse;               // RimWorld universal objects are here (like 'Building')
using Verse.AI;          // Needed when you do something with the AI
using RimWorld;            // RimWorld specific functions are found here (like 'Building_Battery')
using System;
//using RimWorld.SquadAI;  // RimWorld specific functions for squad brains 

namespace SCP
{
    public class JobDriver_HaulSCP : JobDriver
    {
        private const TargetIndex TakeeIndex = TargetIndex.A;
        private const TargetIndex DestinationIndex = TargetIndex.B;
        private string customString = "";

        protected SCP Takee => (SCP)base.job.GetTarget(TargetIndex.A).Thing;

        protected IntVec3 DropLocation => base.job.GetTarget(TargetIndex.B).Cell;

        [DebuggerHidden]
        protected override IEnumerable<Toil> MakeNewToils()
        {
            //Commence fail checks!
            this.FailOnDestroyedOrNull(TargetIndex.A);

            yield return Toils_Reserve.Reserve(TakeeIndex, 1);
            yield return Toils_Reserve.Reserve(DestinationIndex, 1);

            yield return new Toil
            {
                initAction = delegate
                {
                    this.customString = "SCP_GatheringSCPForDrop".Translate();
                }
            };

            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnSomeonePhysicallyInteracting(TargetIndex.A);
            yield return Toils_Construct.UninstallIfMinifiable(TargetIndex.A).FailOnSomeonePhysicallyInteracting(TargetIndex.A);
            yield return Toils_Haul.StartCarryThing(TargetIndex.A);
            yield return Toils_Goto.GotoCell(TargetIndex.B, PathEndMode.ClosestTouch);
            Toil droppingTime = new Toil()
            {
                defaultCompleteMode = ToilCompleteMode.Delay,
                defaultDuration = 100
            };
            droppingTime.WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
            droppingTime.initAction = delegate
            {
                this.customString = "SCP_SCPDropping".Translate(this.Takee.LabelShort
                    );
                Find.World.GetComponent<WorldComponent_UniqueTracker>().uniquePawns.RemoveAll(x => x == Takee);
            };
            yield return droppingTime;
            yield return new Toil
            {
                initAction = delegate
                {
                    this.customString = "SCP_SCPDropFinished".Translate();
                    IntVec3 position = this.DropLocation;
                    this.pawn.carryTracker.TryDropCarriedThing(position, ThingPlaceMode.Direct, out Thing thing, null);
                    
                        SacrificeCompleted();
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };

            yield return Toils_Reserve.Release(TargetIndex.B);

            //Toil 9: Think about that.
            yield return new Toil
            {
                initAction = delegate
                {
                    ////It's a day to remember
                    //TaleDef taleToAdd = TaleDef.Named("HeldSermon");
                    //if ((this.pawn.IsColonist || this.pawn.HostFaction == Faction.OfPlayer) && taleToAdd != null)
                    //{
                    //    TaleRecorder.RecordTale(taleToAdd, new object[]
                    //    {
                    //       this.pawn,
                    //    });
                    //}
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };

            yield break;


        }

        public override string GetReport()
        {
            if (this.customString == "")
            {
                return base.GetReport();
            }
            return this.customString;
        }


        private void SacrificeCompleted()
        {
            //Drop them in~~
            this.Takee.isMoving = false;

            this.Takee.Position = this.DropLocation;
            this.Takee.Notify_Teleported(false);
            this.Takee.stances.CancelBusyStanceHard();

            //Record a tale
            //TaleRecorder.RecordTale(TaleDefOf.ExecutedPrisoner, new object[]
            //{
            //            this.pawn,
            //            this.Takee
            //});
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }
    }
}
