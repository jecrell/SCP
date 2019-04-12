using Verse;

namespace SCP
{
    public abstract class AcceptWorker
    {
        public SCP.FactionDef factionDef;

        public virtual bool CanAccept(Pawn representative)
        {
            return true;
        }

        public virtual void PostAccept(Pawn representative)
        {

        }

        public virtual void PostDecline(Pawn representative)
        {

        }

        public virtual void PostIgnore(Pawn representative)
        {

        }
    }
}