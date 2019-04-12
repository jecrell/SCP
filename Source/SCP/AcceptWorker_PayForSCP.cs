using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace SCP
{
    public class AcceptWorker_PayForSCP : AcceptWorker
    {
        public override void PostAccept(Pawn representative)
        {
            var map = representative.MapHeld;
            if (map == null) return;
            var listOfSCP = map.mapPawns.AllPawnsSpawned.FindAll(x => x?.def?.defName == "SCP_ONE_THREE_ONE_RACE");

            var compensation = new List<Thing>();
            foreach (Pawn scp in listOfSCP)
            {
                var scpLoc = scp.PositionHeld;
                scp.Destroy();
                var silver = ThingMaker.MakeThing(ThingDefOf.Silver);
                silver.stackCount = Rand.Range(1000, 2000);
                GenPlace.TryPlaceThing(silver, scpLoc, representative.MapHeld, ThingPlaceMode.Near);
                compensation.Add(silver);
            }
            Find.LetterStack.ReceiveLetter("SCP_Compensation".Translate(), "SCP_CompensationDesc".Translate(representative.Faction.Name), LetterDefOf.PositiveEvent, compensation);
            base.PostAccept(representative);
        }
    }
}
