#region

using Game.Battle;

#endregion

namespace Game.Logic.Conditons
{
    public interface IICombatUnitCondition
    {
        bool Check(ICombatUnit obj);
    }
}