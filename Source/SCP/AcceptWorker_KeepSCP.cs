using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace SCP
{
    public class AcceptWorker_KeepSCP : AcceptWorker
    {
        public override void PostAccept(Pawn representative)
        {
            var map = representative.MapHeld;
            if (map == null) return;
            var listOfSCP = map.mapPawns.AllPawnsSpawned.FindAll(x => x?.def?.defName == "SCP_ONE_THREE_ONE_RACE");

            foreach (Pawn scp in listOfSCP)
            {
                scp.SetFaction(Faction.OfPlayer);
                scp.training = new Pawn_TrainingTracker(scp);
                //scp.training.Train(TrainableDefOf.Obedience, startingAndOptionalPawn, true);
                //scp.playerSettings.Master = startingAndOptionalPawn;
            }
            Messages.Message("SCP_SCPEntrusted".Translate(), listOfSCP, MessageTypeDefOf.PositiveEvent);

            base.PostAccept(representative);
        }
    }
}
