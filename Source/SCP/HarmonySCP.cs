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
        }

        
    }
}
