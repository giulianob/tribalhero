#region

using Game.Battle.CombatObjects;

#endregion

namespace Game.Logic.Conditons
{
    public interface IICombatUnitCondition
    {
        bool Check(ICombatObject obj);
    }
}