using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace SCP
{
    public class Dialog_WithRepresentative : Dialog_MessageBox
    {
        public Dialog_WithRepresentative(Pawn representative,
            CustomFactionDef factionDef) : base(factionDef.representativeMessage)
        {
            this.text = factionDef.representativeMessage + "\n\n" + "SCP_DialogNote".Translate();

            this.buttonAText = "Ignore".Translate();
            this.buttonAAction = () => factionDef.AcceptWorker.PostIgnore(representative);

            this.buttonBText = "Yes".Translate();
            this.buttonBAction = (() => 
            {
                if (factionDef.AcceptWorker.CanAccept(representative))
                {
                    factionDef.AcceptWorker.PostAccept(representative);

                    var factionsTracker = Find.World.GetComponent<WorldComponent_FactionsTracker>();
                    if (factionsTracker.activeRepresentatives.Contains(representative))
                    {
                        factionsTracker.activeRepresentatives.Clear();
                        factionsTracker.activeRepresentatives = new List<Pawn>();
                        factionsTracker.joinedFactionDef = (CustomFactionDef)representative.Faction.def;
                    }
                }
            });
            this.buttonCText = "No".Translate();
            this.buttonCAction = (() =>
            {
                factionDef.AcceptWorker.PostDecline(representative);

                var factionsTracker = Find.World.GetComponent<WorldComponent_FactionsTracker>();
                if (factionsTracker.activeRepresentatives.Contains(representative))
                    factionsTracker.activeRepresentatives.Remove(representative);
            });
            this.title = "SCP_Proposal".Translate(factionDef.LabelCap);

            this.acceptAction = (() =>
            {
                factionDef.AcceptWorker.PostAccept(representative);

                var factionsTracker = Find.World.GetComponent<WorldComponent_FactionsTracker>();
                if (factionsTracker.activeRepresentatives.Contains(representative))
                        factionsTracker.activeRepresentatives.Remove(representative);
            });
            this.cancelAction = () => factionDef.AcceptWorker.PostIgnore(representative);
            if (buttonAText.NullOrEmpty())
            {
                this.buttonAText = "OK".Translate();
            }
            forcePause = true;
            absorbInputAroundWindow = true;
            //creationRealTime = RealTime.LastRealTime;
            onlyOneOfTypeAllowed = false;
            bool flag = buttonAAction == null && buttonBAction == null && buttonCAction == null;
            forceCatchAcceptAndCancelEventEvenIfUnfocused = (acceptAction != null || cancelAction != null || flag);
            closeOnAccept = flag;
            closeOnCancel = flag;
        }

    }
}
