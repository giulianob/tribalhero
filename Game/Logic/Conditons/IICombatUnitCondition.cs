#region

using Game.Battle;
using Game.Battle.CombatObjects;

#endregion

namespace Game.Logic.Conditons
{
    public interface IICombatUnitCondition
    {
        bool Check(ICombatUnit obj);
    }
}