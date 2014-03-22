namespace Game.Module.Remover
{
    public class PlayerSelectorFactory : IPlayerSelectorFactory
    {
        public NewbieIdleSelector CreateNewbieIdleSelector()
        {
            return new NewbieIdleSelector();
        }
    }
}