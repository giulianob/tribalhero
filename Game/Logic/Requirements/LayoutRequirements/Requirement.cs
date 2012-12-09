namespace Game.Logic.Requirements.LayoutRequirements
{
    public class Requirement
    {
        public Requirement(ushort type, byte cmp, byte minLvl, byte maxLvl, byte minDist, byte maxDist)
        {
            Type = type;
            Cmp = cmp;
            MinLvl = minLvl;
            MaxLvl = maxLvl;
            MinDist = minDist;
            MaxDist = maxDist;
        }

        public byte Cmp { get; set; }

        public byte MaxDist { get; set; }

        public byte MaxLvl { get; set; }

        public byte MinDist { get; set; }

        public byte MinLvl { get; set; }

        public ushort Type { get; set; }
    }
}