using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace SCP
{
    public class WorldComponent_UniqueTracker : WorldComponent
    {
        public List<CustomPawnKindDef> uniquePawnTypes = new List<CustomPawnKindDef>();
        public List<Pawn> uniquePawns = new List<Pawn>();

        public WorldComponent_UniqueTracker(World world) : base(world)
        {

        }

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref this.uniquePawnTypes, "uniquePawnTypes", LookMode.Def);
            Scribe_Collections.Look(ref this.uniquePawns, "uniqueThings", LookMode.Reference);
            base.ExposeData();
        }

    }
}
