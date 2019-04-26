using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace SCP
{
    public class PawnTable_SCPs : PawnTable
    {
        public PawnTable_SCPs(PawnTableDef def, Func<IEnumerable<Pawn>> pawnsGetter, int uiWidth, int uiHeight)
            : base(def, pawnsGetter, uiWidth, uiHeight)
        {
        }

        protected override IEnumerable<Pawn> LabelSortFunction(IEnumerable<Pawn> input)
        {
            return from p in input
                   orderby p.Name == null || p.Name.Numerical, (p.Name is NameSingle) ? ((NameSingle)p.Name).Number : 0, p.def.label
                   select p;
        }

        protected override IEnumerable<Pawn> PrimarySortFunction(IEnumerable<Pawn> input)
        {
            return from p in input
                   orderby p.RaceProps.petness descending, p.RaceProps.baseBodySize
                   select p;
        }
    }

}
