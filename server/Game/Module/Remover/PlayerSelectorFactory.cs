using Persistance;

namespace Game.Module.Remover
{
    public class PlayerSelectorFactory : IPlayerSelectorFactory
    {
        private readonly IDbManager dbManager;

        public PlayerSelectorFactory(IDbManager dbManager)
        {
            this.dbManager = dbManager;
        }

        public NewbieIdleSelector CreateNewbieIdleSelector()
        {
            return new NewbieIdleSelector(dbManager);
        }
    }
}