namespace Game.Logic.Requirements.LayoutRequirements
{
    public class Requirement
    {
        public Requirement()
        {
            
        }

        public Requirement(ushort type, byte cmp, byte minLvl, byte maxLvl, byte minDist, byte maxDist)
        {
            Type = type;
            Cmp = cmp;
            MinLvl = minLvl;
            MaxLvl = maxLvl;
            MinDist = minDist;
            MaxDist = maxDist;
        }

        public virtual byte Cmp { get; set; }
        
        public virtual byte MaxDist { get; set; }
        
        public virtual byte MaxLvl { get; set; }
        
        public virtual byte MinDist { get; set; }
        
        public virtual byte MinLvl { get; set; }

        public virtual ushort Type { get; set; }
    }
}