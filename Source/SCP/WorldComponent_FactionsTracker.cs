using RimWorld.Planet;
using Verse;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SCP
{
    public class WorldComponent_FactionsTracker : WorldComponent
    {
        public bool firstSCPSpawned = false;
        public List<SCP.FactionDef> factionsLeftToSpawn = new List<SCP.FactionDef>();
        public SCP.FactionDef joinedFaction = null;
        public int ticksUntilNextReveal = int.MaxValue;
        public int ticksUntilSCPArrival = int.MaxValue;
        public List<Pawn> activeRepresentatives = new List<Pawn>();

        private static int GetInitialSCPArrivalTime =>
            new IntRange((int)(GenDate.TicksPerDay * 3), (int)(GenDate.TicksPerDay * 6)).RandomInRange;

        private static int GetNextRevealInterval =>
             new IntRange((int)(GenDate.TicksPerDay * 0.2), (int)(GenDate.TicksPerDay * 0.6)).RandomInRange;

        public override void WorldComponentTick()
        {
            if (!firstSCPSpawned &&
                Find.TickManager.TicksGame > ticksUntilSCPArrival)
            {
                firstSCPSpawned = true;
                SCPUtility.SpawnFirstSCPGroup(Find.AnyPlayerHomeMap);
                factionsLeftToSpawn = new List<FactionDef>(DefDatabase<SCP.FactionDef>.AllDefsListForReading);
                ticksUntilNextReveal = Find.TickManager.TicksGame + GetNextRevealInterval;
            }
            if (factionsLeftToSpawn?.Count > 0 && firstSCPSpawned &&
                Find.TickManager.TicksGame > ticksUntilNextReveal)
            {
                ticksUntilNextReveal = Find.TickManager.TicksGame + GetNextRevealInterval;
                SCP.FactionDef toBeRevealed = factionsLeftToSpawn.Pop();
                FactionUtility.IntroduceFaction(toBeRevealed);
            }
            base.WorldComponentTick();
        }

        
        public WorldComponent_FactionsTracker(World world) : base(world)
        {
            ticksUntilSCPArrival = Find.TickManager.TicksGame + GetInitialSCPArrivalTime;
            Log.Message($"Ticks until SCP arrival: {ticksUntilSCPArrival}");
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref this.firstSCPSpawned, "firstSCPSpawned", false);
            Scribe_Collections.Look(ref this.factionsLeftToSpawn, "factionsLeftToSpawn", LookMode.Def);
            Scribe_Defs.Look(ref this.joinedFaction, "joinedFaction");
            Scribe_Values.Look(ref this.ticksUntilNextReveal, "ticksUntilNextReveal", int.MaxValue);
            Scribe_Values.Look(ref this.ticksUntilSCPArrival, "ticksUntilSCPArrival", int.MaxValue);
            Scribe_Collections.Look(ref this.activeRepresentatives, "activeRepresentatives", LookMode.Reference);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                activeRepresentatives.RemoveAll((Pawn x) => x == null);
            }
            base.ExposeData();
        }
    }
}
