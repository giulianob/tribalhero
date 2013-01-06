using Game.Map;

namespace Game.Logic.Procedures
{
    public partial class Procedure
    {
        private readonly IRegionManager regions;

        public Procedure()
        {
        }

        public Procedure(IRegionManager regions)
        {
            this.regions = regions;
        }

        public static Procedure Current { get; set; }
    }
}