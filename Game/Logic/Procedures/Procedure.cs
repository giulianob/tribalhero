using Persistance;

namespace Game.Logic.Procedures
{
    public partial class Procedure
    {
        private readonly IDbManager dbManager;

        public static Procedure Current { get; set; }

        public Procedure()
        {
            
        }

        public Procedure(IDbManager dbManager)
        {
            this.dbManager = dbManager;
        }
    }
}