using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI.Group;

namespace SCP
{
    public class FactionUtility
    {
        
        public static void IntroduceFaction(SCP.FactionDef newFaction)
        {

            Faction faction = SpawnNewFactionIntoWorld(newFaction);
            if (faction == null) return;

            SpawnNewFactionBasesIntoWorld(faction, 3);
            
            //Prevents errors where pawns will refuse to walk around and throw errors.
            //  This is primarily due to the game not being designed for
            //  dynamic faction introduction.
            foreach (Map m in Find.Maps)
            {
                m.pawnDestinationReservationManager.RegisterFaction(faction);
            }

            var map = Find.AnyPlayerHomeMap;
            IntVec3 result;
            if (!RCellFinder.TryFindRandomPawnEntryCell(out result, map, CellFinder.EdgeRoadChance_Hostile))
            {
                Log.Error($"Failed to find spawning point along map edge for {faction.Name}");
            }

            PawnGenerationRequest request = new PawnGenerationRequest(newFaction.representativeKind, faction, PawnGenerationContext.NonPlayer, -1, forceGenerateNewPawn: true, newborn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: true);
            Pawn pawn = null;
            try
            {
                pawn = PawnGenerator.GeneratePawn(request);
            }
            catch
            {
                Log.Error($"Failed to generate a representative pawn for {faction.Name}");
            }
            pawn.relations.everSeenByPlayer = true;
            PawnComponentsUtility.AddComponentsForSpawn(pawn);
            GenSpawn.Spawn(pawn, result, map);
            IntVec3 chillSpot;
            RCellFinder.TryFindRandomSpotJustOutsideColony(pawn, out chillSpot);
            LordJob_VisitColony lordJob = new LordJob_VisitColony(faction, chillSpot);
            LordMaker.MakeNewLord(faction, lordJob, map, new List<Pawn>() {pawn });

            var ft = Find.World.GetComponent<WorldComponent_FactionsTracker>();
            if (ft.joinedFaction == null)
                ft.activeRepresentatives.Add(pawn);
            else
                Messages.Message("SCP_AlreadyJoined".Translate(pawn.Faction.def.label), pawn, MessageTypeDefOf.RejectInput);

            Find.LetterStack.ReceiveLetter(
            "SCP_FactionArrival".Translate(),
            "SCP_FactionArrivalDesc".Translate(newFaction.LabelCap),
            LetterDefOf.NeutralEvent, pawn);

        }

        public static Faction SpawnNewFactionIntoWorld(SCP.FactionDef newFaction)
        {
            if (Find.FactionManager.AllFactionsListForReading.Any(x => x.def == newFaction))
            {
                Log.Error($"Attempted to spawn existing faction, {newFaction.defName}, into the world.");
                return null;
            }
            Faction faction = FactionGenerator.NewGeneratedFaction(newFaction);
            Find.FactionManager.Add(faction);
            return faction;
        }


        public static void SpawnNewFactionBasesIntoWorld(Faction newFaction, int numOfBases)
        {
            if (newFaction == null) return;

            for (int k = 0; k < numOfBases; k++)
            {
                Settlement settlement = (Settlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
                settlement.SetFaction(newFaction);
                settlement.Tile = TileFinder.RandomSettlementTileFor(newFaction);
                settlement.Name = SettlementNameGenerator.GenerateSettlementName(settlement);
                Find.WorldObjects.Add(settlement);
            }
        }
    }
}
