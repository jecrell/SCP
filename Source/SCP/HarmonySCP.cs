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
            harmony.Patch(AccessTools.Method(typeof(GenSpawn), "Spawn", new Type[] { typeof(Thing), typeof(IntVec3), typeof(Map), typeof(Rot4), typeof(WipeMode), typeof(bool) }), new HarmonyMethod(typeof(HarmonyPatches), nameof(SCP_SpawnCheck)), null);
            harmony.Patch(AccessTools.Method(typeof(FloatMenuMakerMap), "AddHumanlikeOrders"),
                null, new HarmonyMethod(typeof(HarmonyPatches), nameof(AddHumanlikeOrders)));
            harmony.Patch(AccessTools.Method(typeof(PawnRenderer), "RenderPawnAt", new[] { typeof(Vector3), typeof(RotDrawMode), typeof(bool) }),
                null, new HarmonyMethod(typeof(HarmonyPatches), nameof(RenderPawnAt)));
            harmony.Patch(original: AccessTools.Method(type: typeof(GameRules), name: nameof(GameRules.DesignatorAllowed)), prefix: null,
                   postfix: new HarmonyMethod(typeof(HarmonyPatches), name: nameof(DesignatorAllowedPostfix)));
            harmony.Patch(original: AccessTools.Method(type: typeof(GenConstruct), name: nameof(GenConstruct.CanConstruct)), prefix: null,
                postfix: new HarmonyMethod(typeof(HarmonyPatches), name: nameof(CanConstructPostfix)));

            //Add SCPs to the "Animals" Tab
            //harmony.Patch(AccessTools.Method(typeof(MainTabWindow_Animals), "get_Pawns"), null,
            //    new HarmonyMethod(typeof(HarmonyPatches), nameof(AnimalsPawnsPostFix)));

            //Allow for trainable mechanoids (mechanoid SCPs that can move around)
            harmony.Patch(original: AccessTools.Method(type: typeof(WildManUtility), name: nameof(WildManUtility.AnimalOrWildMan)), prefix: null,
                postfix: new HarmonyMethod(typeof(HarmonyPatches), name: nameof(AnimalOrWildMan_PostFix)));
        }
        
        public static IEnumerable<Pawn> SCPGetter(Map map)
        {

                var enumerator = map.mapPawns.PawnsInFaction(Faction.OfPlayer).GetEnumerator();
                while (enumerator.MoveNext())
                {
                    var current = enumerator.Current;
                    if (current.kindDef is CustomPawnKindDef)
                        yield return current;
                    continue;
                }
        }

        public static void AnimalsPawnsPostFix(ref IEnumerable<Pawn> __result)
        {
            var newPawnsToAdd = new List<Pawn>(SCPGetter(Find.CurrentMap));
            if (!newPawnsToAdd.NullOrEmpty())
            {
                __result = __result.Concat(newPawnsToAdd);
            }
        }

        // Verse.WildManUtility
        public static void AnimalOrWildMan_PostFix(Pawn p, ref bool __result)
        {
            if (p.kindDef is CustomPawnKindDef spckind && spckind.trainableMechanoid)
            {
                Log.Message("SCP Check passed for " + p.Label);
                __result = true;
            }
        }


        private static int colonistRacesTick;
        private const int COLONIST_RACES_TICK_TIMER = GenDate.TicksPerHour * 2;
        public static Dictionary<string, CustomFactionDef> uniqueFactionBuildings;



        public static void DesignatorAllowedPostfix(Designator d, ref bool __result)
        {
            if (!__result || !(d is Designator_Build build)) return;
            __result = CanBuild(__result, build.PlacingDef.defName);
        }
        public static void CanConstructPostfix(Thing t, Pawn p, ref bool __result)
        {
            if (!__result) return;
            __result = CanBuild(__result, t?.def?.entityDefToBuild?.defName ?? t.def.defName);
        }

        private static bool CanBuild(bool __result, string build)
        {
            //Initialize Unique Faction Buildings List
            if (uniqueFactionBuildings == null)
            {
                uniqueFactionBuildings = new Dictionary<string, CustomFactionDef>();
                foreach (var facDef in DefDatabase<CustomFactionDef>.AllDefs)
                {
                    foreach (var bld in facDef.factionBuildings)
                    {
                        uniqueFactionBuildings.Add(bld, facDef);
                    }
                }
            }

            //This shouldn't happen...
            if (uniqueFactionBuildings.Count == 0 || uniqueFactionBuildings == null)
                Log.Error("SCP :: Unique faction buildings list is 0");

            //Is it a unique faction building? Don't build it.
            if (uniqueFactionBuildings.ContainsKey(build))
            {
                __result = false;

                //Unless we joined that faction
                if (Find.World.GetComponent<WorldComponent_FactionsTracker>().joinedFactionDef is CustomFactionDef fac)
                {
                    if (uniqueFactionBuildings[build] == fac)
                    {
                        __result = true;
                    }
                }
            }

            return __result;
        }


        private static WorldComponent_FactionsTracker worldComponent_FactionsTracker = null;
        private static WorldComponent_FactionsTracker FactionTracker
        {
            get
            {
                if (worldComponent_FactionsTracker == null)
                    worldComponent_FactionsTracker = Find.World.GetComponent<WorldComponent_FactionsTracker>();
                return worldComponent_FactionsTracker;
            }
        }

        //PawnRenderer
        public static void RenderPawnAt(PawnRenderer __instance, Vector3 drawLoc, RotDrawMode bodyDrawType, bool headStump)
        {
            var pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            if (pawn != null && IsRepresentative(pawn) && FactionTracker.activeRepresentatives.Contains(pawn)) RenderExclamationPointOverlay(pawn);
        }

        public static bool IsRepresentative(Pawn pawn)
        {
            if (pawn == null) return false;
            if (pawn.kindDef == null) return false;
            if (pawn.kindDef.defName == String.Empty) return false;


            if (pawn.Faction == null) return false;
            if (pawn?.Faction?.def?.defName?.StartsWith("SCP_") == true)
            {
                if (pawn.kindDef.defName.EndsWith("Representative"))
                {
                    return true;
                }
            }
            return false;
        }


        private static TargetingParameters ForTalk()
        {
            var targetingParameters = new TargetingParameters
            {
                canTargetPawns = true,
                canTargetBuildings = false,
                mapObjectTargetsMustBeAutoAttackable = false,
                validator = x =>
                    x.Thing is Pawn pawn &&
                    IsRepresentative(pawn) &&
                    FactionTracker.activeRepresentatives.Contains(pawn)
            };
            return targetingParameters;
        }


        //FloatMenuMakerMap
        public static void AddHumanlikeOrders(Vector3 clickPos, Pawn pawn, List<FloatMenuOption> opts)
        {
            foreach (var localTargetInfo4 in GenUI.TargetsAt(clickPos, ForTalk(), true))
            {
                var dest = localTargetInfo4;
                if (!pawn.CanReach(dest, PathEndMode.OnCell, Danger.Deadly))
                {
                    opts.Add(new FloatMenuOption("SCP_CannotConverse".Translate() + " (" + "NoPath".Translate() + ")", null));
                }
                else if (pawn.skills.GetSkill(SkillDefOf.Social).TotallyDisabled)
                {
                    opts.Add(new FloatMenuOption("CannotPrioritizeWorkTypeDisabled".Translate(SkillDefOf.Social.LabelCap), null));
                }
                else
                {
                    var pTarg = (Pawn)dest.Thing;
                    void Action4()
                    {
                        var job = new Job(DefDatabase<JobDef>.GetNamed("SCP_TalkWithRepresentative"), pTarg);
                        job.playerForced = true;
                        pawn.jobs.TryTakeOrderedJob(job);
                    }
                    var str = string.Empty;
                    if (pTarg.Faction != null)
                        str = " (" + pTarg.Faction.Name + ")";
                    var label = "SCP_MeetWith".Translate(pTarg.LabelShort) + str;
                    var action = (Action)Action4;
                    var priority2 = MenuOptionPriority.InitiateSocial;
                    var thing = dest.Thing;
                    opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(label, action, priority2, null, thing), pawn, pTarg));
                }
            }
        }


        private static void RenderExclamationPointOverlay(Thing t)
        {
            if (!t.Spawned) return;
            var drawPos = t.DrawPos;
            drawPos.y = Altitudes.AltitudeFor(AltitudeLayer.MetaOverlays) + 0.28125f;
            if (t is Pawn)
            {
                drawPos.x += (float)t.def.size.x - 0.52f;
                drawPos.z += (float)t.def.size.z - 0.45f;
            }
            RenderPulsingOverlayQuest(t, HarmonyPatches.ExclamationPointMat, drawPos, MeshPool.plane05);
        }

        private static readonly Material ExclamationPointMat =
            MaterialPool.MatFrom("UI/Overlays/SCP_ExclamationPoint", ShaderDatabase.MetaOverlay);

        private static void RenderPulsingOverlayQuest(Thing thing, Material mat, Vector3 drawPos, Mesh mesh)
        {
            var num = (Time.realtimeSinceStartup + 397f * (float)(thing.thingIDNumber % 571)) * 4f;
            var num2 = ((float)Math.Sin((double)num) + 1f) * 0.5f;
            num2 = 0.3f + num2 * 0.7f;
            var material = FadedMaterialPool.FadedVersionOf(mat, num2);
            Graphics.DrawMesh(mesh, drawPos, Quaternion.identity, material, 0);
        }

        public static bool SCP_SpawnCheck(Thing newThing, IntVec3 loc, Map map, Rot4 rot, WipeMode wipeMode, bool respawningAfterLoad)
        {
            if (newThing is Pawn newPawn && newPawn.kindDef is CustomPawnKindDef newPkd && newPkd.isUnique)
            {
                //SCPs should not be spawned as mechanoids in shrines.
                //Spawn lancers instead.
                if (Find.TickManager.TicksGame < 500)
                {
                    newThing = PawnGenerator.GeneratePawn(CustomPawnKindDef.Named("Mech_Lancer"));
                    //Log.Message(newPkd.label + " attempted to spawn before gamestart. Denied");
                    return true;
                }

                var worldComp = Find.World.GetComponent<WorldComponent_UniqueTracker>();
                if (worldComp == null) return true;

                if (worldComp.uniquePawnTypes == null)
                    worldComp.uniquePawnTypes = new List<CustomPawnKindDef>();

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
