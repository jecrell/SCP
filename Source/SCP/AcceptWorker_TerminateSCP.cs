using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace SCP
{
    public class AcceptWorker_TerminateSCP : AcceptWorker
    {

        public override bool CanAccept(Pawn representative)
        {
            var result = Find.ResearchManager.currentProj != null &&
                         !Find.ResearchManager.currentProj.IsFinished;
            if (result == true)
                return true;
            Messages.Message("SCP_MustHaveACurrentResearchProject".Translate(), MessageTypeDefOf.RejectInput);
            return false;
        }

        public override void PostAccept(Pawn representative)
        {
            var map = representative.MapHeld;
            if (map == null) return;
            var listOfSCP = map.mapPawns.AllPawnsSpawned.FindAll(x => x?.def?.defName == "SCP_ONE_THREE_ONE_RACE");

            foreach (Pawn scp in listOfSCP)
            {
                scp.Destroy();
            }


            Find.ResearchManager.FinishProject(Find.ResearchManager.currentProj, true, representative.Faction.leader);
        }
    }
}
