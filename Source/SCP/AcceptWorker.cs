namespace SCP
{
    public abstract class AcceptWorker
    {
        public SCP.FactionDef factionDef;
        public virtual void PostAccept()
        {

        }

        public virtual void PostDecline()
        {

        }

        public virtual void PostIgnore()
        {

        }
    }
}