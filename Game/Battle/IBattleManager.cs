using System.Collections.Generic;
using Game.Comm;
using Game.Data;
using Game.Data.Troop;
using Persistance;

namespace Game.Battle
{
    public interface IBattleManager : IPersistableObject
    {
        uint BattleId { get; set; }
        bool BattleStarted { get; set; }
        uint Round { get; set; }
        uint Turn { get; set; }
        City City { get; set; }
        CombatList Attacker { get; }
        CombatList Defender { get; }
        IBattleReport BattleReport { get; }
        ReportedObjects ReportedObjects { get; }
        ReportedTroops ReportedTroops { get; }
        City[] LockList { get; }
        void Subscribe(Session session);
        void Unsubscribe(Session session);
        CombatObject GetCombatObject(uint id);
        bool CanWatchBattle(Player player, out int roundsLeft);
        void DbLoaderAddToLocal(CombatStructure structure, uint id);
        void DbLoaderAddToCombatList(CombatObject obj, uint id, bool isLocal);
        void AddToLocal(IEnumerable<TroopStub> objects, ReportState state);
        void AddToLocal(IEnumerable<Structure> objects);
        void AddToAttack(TroopStub stub);
        void AddToAttack(IEnumerable<TroopStub> objects);
        void AddToDefense(IEnumerable<TroopStub> objects);
        void RemoveFromAttack(IEnumerable<TroopStub> objects, ReportState state);
        void RemoveFromLocal(IEnumerable<Structure> objects, ReportState state);
        void RemoveFromDefense(IEnumerable<TroopStub> objects, ReportState state);
        void RefreshBattleOrder();
        bool GroupIsDead(CombatObject co, CombatList combatList);
        bool ExecuteTurn();
        event BattleManager.OnBattle EnterBattle;
        event BattleManager.OnBattle ExitBattle;
        event BattleManager.OnRound EnterRound;
        event BattleManager.OnTurn EnterTurn;
        event BattleManager.OnTurn ExitTurn;
        event BattleManager.OnReinforce ReinforceAttacker;
        event BattleManager.OnReinforce ReinforceDefender;
        event BattleManager.OnReinforce WithdrawAttacker;
        event BattleManager.OnReinforce WithdrawDefender;
        event BattleManager.OnUnitUpdate UnitAdded;
        event BattleManager.OnUnitUpdate UnitRemoved;
        event BattleManager.OnUnitUpdate UnitUpdated;
        event BattleManager.OnUnitUpdate SkippedAttacker;
        event BattleManager.OnAttack ActionAttacked;
    }
}