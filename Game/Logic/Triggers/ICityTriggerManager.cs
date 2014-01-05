using Game.Logic.Conditons;

namespace Game.Logic.Triggers
{
    public interface ICityTriggerManager
    {
        void AddTrigger(IDynamicCondition dynamicCondition, IDynamicAction action);

        void Process(ICityEvent cityEvent);
    }
}