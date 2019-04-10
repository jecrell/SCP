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
            SCP.FactionDef factionDef) : base(factionDef.representativeMessage)
        {
            this.text = factionDef.representativeMessage;
            this.buttonAText = "Yes".Translate();
            this.buttonAAction = factionDef.AcceptWorker.PostAccept;
            this.buttonBText = "No".Translate();
            this.buttonBAction = factionDef.AcceptWorker.PostDecline;
            this.title = "PawnMainDescFactionedWrap".Translate(representative.KindLabel, factionDef.LabelCap);
            this.buttonCText = "Ignore".Translate();
            this.buttonCAction = factionDef.AcceptWorker.PostIgnore;
            this.acceptAction = factionDef.AcceptWorker.PostAccept;
            this.cancelAction = factionDef.AcceptWorker.PostIgnore;
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
