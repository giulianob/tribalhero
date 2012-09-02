using Game.Map;

namespace Game.Logic.Procedures
{
    public partial class Procedure
    {
        private readonly IWorld world;

        public static Procedure Current { get; set; }

        public Procedure()
        {
            
        }

        public Procedure(IWorld world)
        {
            this.world = world;
        }
    }
}