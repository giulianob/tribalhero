using System.Collections.Generic;
using Game.Battle.CombatGroups;
using Game.Battle.CombatObjects;
using Game.Battle.Reporting;
using Game.Data;
using Game.Util.Locking;
using Persistance;

namespace Game.Battle
{
    public interface IBattleManager : IPersistableObject
    {
        uint BattleId { get; }
        bool BattleStarted { get; set; }
        uint Round { get; set; }
        uint Turn { get; set; }
        BattleLocation Location { get; }
        BattleOwner Owner { get; }
        ICombatList Attackers { get; }
        ICombatList Defenders { get; }
        IBattleReport BattleReport { get; }
        IEnumerable<ILockable> LockList { get; }
        BattleManager.BattleSide NextToAttack { set; }

        ICombatObject GetCombatObject(uint id);
        ICombatGroup GetCombatGroup(uint id);
        bool CanWatchBattle(IPlayer player, out int roundsLeft);
        void DbLoaderAddToCombatList(ICombatGroup group, BattleManager.BattleSide side);
        void Add(ICombatGroup combatGroup, BattleManager.BattleSide battleSide);
        void Remove(ICombatGroup group, BattleManager.BattleSide side, ReportState state);
        bool ExecuteTurn();        

        event BattleManager.OnBattle EnterBattle;
        event BattleManager.OnBattle ExitBattle;
        event BattleManager.OnRound EnterRound;
        event BattleManager.OnTurn ExitTurn;
        event BattleManager.OnReinforce ReinforceAttacker;
        event BattleManager.OnReinforce ReinforceDefender;
        event BattleManager.OnWithdraw WithdrawAttacker;
        event BattleManager.OnWithdraw WithdrawDefender;
        event BattleManager.OnUnitUpdate UnitKilled;
        event BattleManager.OnUnitUpdate SkippedAttacker;
        event BattleManager.OnAttack ActionAttacked;

        uint GetNextGroupId();

        uint GetNextCombatObjectId();

        void DbFinishedLoading();
    }
}