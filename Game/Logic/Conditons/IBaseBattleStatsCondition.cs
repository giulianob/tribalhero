#region

using Game.Data;

#endregion

namespace Game.Logic.Conditons
{
    public interface IBaseBattleStatsCondition
    {
        bool Check(BaseBattleStats obj);
    }
}