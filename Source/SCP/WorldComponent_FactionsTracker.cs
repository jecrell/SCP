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

        public override void WorldComponentTick()
        {
            if (!firstSCPSpawned &&
                Find.TickManager.TicksGame > ticksUntilSCPArrival)
            {
                firstSCPSpawned = true;
                SCPUtility.SpawnFirstSCPGroup(Find.AnyPlayerHomeMap);
                factionsLeftToSpawn = new List<FactionDef>(DefDatabase<SCP.FactionDef>.AllDefsListForReading);
                ticksUntilNextReveal = Find.TickManager.TicksGame + (new IntRange(GenDate.TicksPerDay, (int)(GenDate.TicksPerDay * 1.5)).RandomInRange);
            }
            if (factionsLeftToSpawn?.Count > 0 && firstSCPSpawned &&
                Find.TickManager.TicksGame > ticksUntilNextReveal)
            {
                ticksUntilNextReveal = Find.TickManager.TicksGame + (new IntRange(GenDate.TicksPerDay, (int)(GenDate.TicksPerDay * 1.5)).RandomInRange);
                SCP.FactionDef toBeRevealed = factionsLeftToSpawn.Pop();
                FactionUtility.IntroduceFaction(toBeRevealed);
            }
            base.WorldComponentTick();
        }

        public WorldComponent_FactionsTracker(World world) : base(world)
        {
            ticksUntilSCPArrival = Find.TickManager.TicksGame + (new IntRange(GenDate.TicksPerDay, (int)(GenDate.TicksPerDay * 1.5)).RandomInRange);
            Log.Message($"Ticks until SCP arrival: {ticksUntilSCPArrival}");
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref this.firstSCPSpawned, "firstSCPSpawned", false);
            Scribe_Values.Look(ref this.ticksUntilNextReveal, "ticksUntilNextReveal", int.MaxValue);
            Scribe_Values.Look(ref this.ticksUntilSCPArrival, "ticksUntilSCPArrival", int.MaxValue);
            Scribe_Collections.Look(ref this.factionsLeftToSpawn, "factionsLeftToSpawn", LookMode.Def);
            Scribe_Defs.Look(ref this.joinedFaction, "joinedFaction");
            base.ExposeData();
        }
    }
}
