using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace SCP
{
    public class MainTabWindow_SCPs : MainTabWindow_PawnTable
    {
        protected override PawnTableDef PawnTableDef => DefDatabase<PawnTableDef>.GetNamed("SCP_SCPPawnTable");

        protected override IEnumerable<Pawn> Pawns => from p in Find.CurrentMap.mapPawns.AllPawnsSpawned
                                                      where p.kindDef is CustomPawnKindDef
                                                      select p;
        
        public override void PostOpen()
        {
            base.PostOpen();
            Find.World.renderer.wantedMode = WorldRenderMode.None;
        }
    }
}
