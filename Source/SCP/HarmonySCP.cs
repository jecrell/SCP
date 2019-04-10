using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using System.Reflection;
using UnityEngine;
using Verse.AI.Group;

namespace SCP
{

    [StaticConstructorOnStartup]
    static class HarmonyPatches
    {
        
        static HarmonyPatches()
        {
            var harmony = HarmonyInstance.Create("rimworld.scp");
            //harmony.Patch(AccessTools.Method(typeof(PawnUIOverlay), "DrawPawnGUIOverlay"),
            //    null, new HarmonyMethod(typeof(HarmonyPatches), nameof(DrawPawnGUIOverlay)));
            harmony.Patch(AccessTools.Method(typeof(GenSpawn), "Spawn", new Type[] { typeof(Thing), typeof(IntVec3), typeof(Map), typeof(Rot4), typeof(WipeMode), typeof(bool) }), new HarmonyMethod(typeof(HarmonyPatches), nameof(SCP_SpawnCheck)), null);
        }

        public static bool SCP_SpawnCheck(Thing newThing, IntVec3 loc, Map map, Rot4 rot, WipeMode wipeMode, bool respawningAfterLoad)
        {
            if (newThing is Pawn newPawn && newPawn.kindDef is SCP.PawnKindDef newPkd && newPkd.isUnique)
            {
                var worldComp = Find.World.GetComponent<WorldComponent_UniqueTracker>();
                if (worldComp == null) return true;

                if (worldComp.uniquePawnTypes == null)
                    worldComp.uniquePawnTypes = new List<PawnKindDef>();

                //Check to see if the unique pawn type has already been spawned.
                if (worldComp.uniquePawnTypes.Count != 0 &&
                    worldComp.uniquePawnTypes.Contains(newPkd))
                {
                    //If the pawn is a newly created or different pawn, it will not be spawned.
                    if (worldComp.uniquePawns == null)
                        worldComp.uniquePawns = new List<Pawn>();

                    if (worldComp.uniquePawns.Count != 0 &&
                        !worldComp.uniquePawns.Contains(newPawn))
                    {
                        Log.Warning("SCP attempted to reappear without permissions. SCP Spawning Denied.");
                        return false;
                    }
                }
                else
                {
                    worldComp.uniquePawnTypes.Add(newPkd);
                    worldComp.uniquePawns.Add(newPawn);
                }
            }
            return true;
        }

        //public static Thing Spawn(Thing newThing, IntVec3 loc, Map map, Rot4 rot, WipeMode wipeMode = WipeMode.Vanish, bool respawningAfterLoad = false)
    }
}
