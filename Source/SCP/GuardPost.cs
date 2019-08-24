using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace SCP
{
    public class GuardPost : Building
    {
        Pawn currentGuard;

        public Pawn CurrentGuard => currentGuard;

        public override void ExposeData()
        {
            Scribe_References.Look(ref this.currentGuard, "currentGuard");
            base.ExposeData();
        }
    }
}
