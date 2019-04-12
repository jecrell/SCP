using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace SCP
{
    public class FactionDef : RimWorld.FactionDef
    {
        public Verse.PawnKindDef representativeKind;
        public string representativeMessage;
        public bool hostileByDefault;
        public List<string> factionBuildings;
        public List<string> hostileToFactions;
        public Type acceptWorker = typeof(AcceptWorker);
        public List<string> acceptMessages;
        public List<string> declineMessages;
        public List<StartingRelation> startingRelations;

        public AcceptWorker AcceptWorker
        {
            get
            {
                if (workerInt == null)
                {
                    workerInt = (AcceptWorker)Activator.CreateInstance(acceptWorker);
                    workerInt.factionDef = this;
                }
                return workerInt;
            }
        }

        [Unsaved]
        private AcceptWorker workerInt;
    }
}
