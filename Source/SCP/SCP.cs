using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace SCP
{
    public class SCP : Pawn
    {
        public bool isMoving;
        
        public static bool IsActorAvailable(Pawn preacher, bool downedAllowed = false)
        {
            if (preacher == null)
                return false;
            if (preacher.Dead)
                return false;
            if (preacher.Downed && !downedAllowed)
                return false;
            if (preacher.Drafted)
                return false;
            if (preacher.InAggroMentalState)
                return false;
            if (preacher.InMentalState)
                return false;
            return true;
        }

        public void ProcessInput()
        {
            if (!this.isMoving)
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                Map map = this.Map;
                List<Pawn> colonists = new List<Pawn>(map.mapPawns.FreeColonistsSpawned);
                if (colonists.Count != 0)
                {
                    foreach (Pawn current in map.mapPawns.FreeColonistsSpawned)
                    {
                        if (!current.Dead)
                        {
                            string text = current.Name.ToStringFull;
                            List<FloatMenuOption> arg_121_0 = list;
                            Func<Rect, bool> extraPartOnGUI = (Rect rect) => Widgets.InfoCardButton(rect.x + 5f, rect.y + (rect.height - 24f) / 2f, current);
                            arg_121_0.Add(new FloatMenuOption(text, delegate
                            {
                                this.TryHaulSCP(current);
                            }, MenuOptionPriority.Default, null, null, 29f, extraPartOnGUI, null));
                        }
                    }
                }
                else
                {
                    list.Add(new FloatMenuOption("Nocolonists".Translate(), delegate
                    {
                    }, MenuOptionPriority.Default));
                }
                Find.WindowStack.Add(new FloatMenu(list));
            }
            else
            {
                TryCancelHaul();
            }
        }

        private void TryCancelHaul(string reason = "")
        {
            Pawn pawn = null;
            List<Pawn> listeners = this.Map.mapPawns.AllPawnsSpawned.FindAll(x => x.RaceProps.intelligence == Intelligence.Humanlike);
            bool[] flag = new bool[listeners.Count];
            for (int i = 0; i < listeners.Count; i++)
            {
                pawn = listeners[i];
                if (pawn.Faction == Faction.OfPlayer)
                {
                    if (pawn.CurJob.def == DefDatabase<JobDef>.GetNamed("SCP_HaulSCP"))
                    {
                        pawn.jobs.StopAll();
                    }
                }
            }
            tempLoc = IntVec3.Invalid;
            this.isMoving = false;
            Messages.Message("Cancelling relocation. " + reason, MessageTypeDefOf.NegativeEvent);
        }

        private AcceptanceReport CanDesignateCell(IntVec3 c)
        {
            if (!c.InBounds(base.Map))
            {
                return false;
            }
            if (!c.Standable(base.Map))
            {
                return false;
            }
            return true;
        }
        
        protected Rot4 placingRot = Rot4.South;
        private IntVec3 tempLoc;


        //private void DrawGhost(Color ghostCol, IntVec3 loc)
        //{
        //    GhostDrawer.DrawGhostThing(loc, placingRot, this.def, null, ghostCol, AltitudeLayer.Blueprint);
        //}

        public override void DrawExtraSelectionOverlays()
        {
            //if (isMoving && tempLoc != null && tempLoc.IsValid)
            //{
                base.DrawExtraSelectionOverlays();
            //    DrawGhost(Color.white, tempLoc);
            //}
        }

        private void StartHaul(Pawn actor)
        {
            if (this.Destroyed || !this.Spawned)
            {
                TryCancelHaul("SCP_Unavailable".Translate());
                return;
            }
            if (!IsActorAvailable(actor))
            {
                TryCancelHaul("SCP_AgentUnavailable".Translate());
                return;
            }



            var targetingParams = new TargetingParameters()
            {
                canTargetLocations = true,
                validator = ((TargetInfo c) => c.Cell is IntVec3 cell && cell.IsValid && cell.Standable(c.Map)),  
            };

            //var texture2D = (Texture2D)GhostUtility.GhostGraphicFor(this.def.graphicData.Graphic, this.def, Color.white).MatSouth.GetTexture(ShaderPropertyIDs.Color);

            SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
            Find.Targeter.BeginTargeting(targetingParams, delegate (LocalTargetInfo target)
            {
                tempLoc = target.Cell;
                this.isMoving = true;
                MakeHaulJobSCP(actor, target.Cell);
                Find.Targeter.StopTargeting();
            });
            
        }

        private void MakeHaulJobSCP(Pawn actor, IntVec3 loc)
        {
            Job job = new Job(DefDatabase<JobDef>.GetNamed("SCP_HaulSCP"), this, loc)
            {
                count = 1
            };
            actor.jobs.TryTakeOrderedJob(job);
        }

        private void TryHaulSCP(Pawn actor)
        {

            if (actor != null)
            {
                StartHaul(actor);
            }
            else
            {
                Messages.Message("SCP_CannotFindActor".Translate(), MessageTypeDefOf.RejectInput);
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            IEnumerator<Gizmo> enumerator = base.GetGizmos().GetEnumerator();
            while (enumerator.MoveNext())
            {
                Gizmo current = enumerator.Current;
                yield return current;
            }

                if (!this.isMoving)
                {
                    Command_Action command_Action = new Command_Action()
                    {
                        action = new Action(this.ProcessInput),
                        defaultLabel = "SCP_CommandHaulSCP".Translate(),
                        defaultDesc = "SCP_CommandHaulSCPDesc".Translate(),
                        hotKey = KeyBindingDefOf.Misc1,
                        icon = TexCommand.Install //ContentFinder<Texture2D>.Get("UI/Commands/Forcolonists", true)
                    };
                    yield return command_Action;
                }
                else
                {
                    Command_Action command_Cancel = new Command_Action()
                    {
                        action = new Action(this.ProcessInput),
                        defaultLabel = "CommandCancelConstructionLabel".Translate(),
                        defaultDesc = "SCP_CommandCancelHaulSCPDesc".Translate(),
                        hotKey = KeyBindingDefOf.Designator_Cancel,
                        icon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel", true)
                    };
                    yield return command_Cancel;
                }
        }
    }
}
