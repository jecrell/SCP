using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace SCP
{
    public class WorkGiver_ManGuardDutyPoint : WorkGiver_Scanner
    {
        public override PathEndMode PathEndMode => PathEndMode.OnCell;

        public override Danger MaxPathDanger(Pawn pawn)
        {
            return Danger.Deadly;
        }

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            foreach (var guardPost in pawn.Map.listerBuildings.AllBuildingsColonistOfClass<GuardPost>())
            {
                yield return guardPost;
            }
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            GuardPost gpost = t as GuardPost;
            if (gpost == null)
            {
                return false;
            }
            Pawn pawn2 = gpost.CurrentGuard;
            if (pawn2 != null)
            {
                if (pawn2 == pawn)
                {
                    return false;
                }
                if ((pawn2.Faction == pawn.Faction || pawn2.HostFaction == pawn.Faction || pawn2.HostFaction == pawn.HostFaction) && !pawn.Map.areaManager.Home[gpost.Position] && IntVec3Utility.ManhattanDistanceFlat(pawn.Position, pawn2.Position) > 15)
                {
                    return false;
                }
                if (!pawn.CanReach(pawn2, PathEndMode.Touch, Danger.Deadly))
                {
                    return false;
                }
            }
            else
            {
                if (pawn.story.WorkTagIsDisabled(WorkTags.Violent))
                {
                    return false;
                }
                if (!pawn.Map.areaManager.Home[gpost.Position])
                {
                    JobFailReason.Is(WorkGiver_FixBrokenDownBuilding.NotInHomeAreaTrans);
                    return false;
                }
            }
            if ((pawn.Position - gpost.Position).LengthHorizontalSquared > 225)
            {
                LocalTargetInfo target = gpost;
                bool ignoreOtherReservations = forced;
                if (!pawn.CanReserve(target, 1, -1, null, ignoreOtherReservations))
                {
                    return false;
                }
            }
            return true;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return new Job(JobDefOf.Flick, t);
        }
    }
}
