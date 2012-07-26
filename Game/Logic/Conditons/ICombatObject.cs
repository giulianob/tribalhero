#region

using Game.Battle;
using Game.Battle.CombatObjects;

#endregion

namespace Game.Logic.Conditons
{
    public interface ICombatObjectCondition
    {
        bool Check(ICombatObject obj);
    }
}