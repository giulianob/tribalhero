using System.Collections.Generic;
using Game.Data;
using Game.Data.Troop;
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
        ICity City { get; }
        BattleLocation Location { get; }
        BattleOwner Owner { get; }
        ICombatList Attackers { get; }
        ICombatList Defender { get; }
        IBattleReport BattleReport { get; }
        IEnumerable<ILockable> LockList { get; }
        CombatObject GetCombatObject(uint id);
        bool CanWatchBattle(IPlayer player, out int roundsLeft);
        void DbLoaderAddToLocal(CombatStructure structure, uint id);
        void DbLoaderAddToCombatList(CombatObject obj, uint id, bool isLocal);
        void AddToLocal(IEnumerable<ITroopStub> objects, ReportState state);
        void AddToLocal(IEnumerable<IStructure> objects);
        void AddToAttack(ITroopStub stub);
        void AddToAttack(IEnumerable<ITroopStub> objects);
        void AddToDefense(IEnumerable<ITroopStub> objects);
        void RemoveFromAttack(IEnumerable<ITroopStub> objects, ReportState state);
        void RemoveFromDefense(IEnumerable<ITroopStub> objects, ReportState state);
        void RefreshBattleOrder();
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