using Game.Data;

namespace Game.Module.Remover
{
    public interface IPlayerSelectorFactory
    {
        NewbieIdleSelector CreateNewbieIdleSelector();

        AllButYouSelector CreateAllButYouSelector(IPlayer you);
    }
}