#region

using Game.Battle;

#endregion

namespace Game.Logic.Conditons
{
    public interface ICombatObjectCondition
    {
        bool Check(CombatObject obj);
    }
}