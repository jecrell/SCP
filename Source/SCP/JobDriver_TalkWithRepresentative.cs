using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace SCP
{
    public class JobDriver_TalkWithRepresentative : JobDriver
    {
        private Pawn Representative => (Pawn)base.TargetThingA;

        public override bool TryMakePreToilReservations(bool yeaaa)
        {
            return this.pawn.Reserve(this.Representative, this.job, 1, -1, null);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).FailOn(() => !CanTalkWithNow(Representative));
            var trade = new Toil();
            trade.initAction = delegate
            {
                var actor = trade.actor;
                if (CanTalkWithNow(Representative))
                    Find.WindowStack.Add(new Dialog_WithRepresentative(Representative, (CustomFactionDef)Representative.Faction.def));
            };
            yield return trade;
            yield break;
        }

        public static bool CanTalkWithNow(Pawn pawn)
        {
            if (pawn.Dead || !pawn.Spawned || !pawn.CanCasuallyInteractNow() ||
                (pawn.Downed || pawn.IsPrisoner || pawn.Faction == Faction.OfPlayer) ||
                (pawn.Faction != null && pawn.Faction.HostileTo(Faction.OfPlayer))) return false;
            var listOfRepresentatives = Find.World.GetComponent<WorldComponent_FactionsTracker>().activeRepresentatives;
            return !listOfRepresentatives.NullOrEmpty() && listOfRepresentatives.Any(x => x == pawn);
        }

    }


}
