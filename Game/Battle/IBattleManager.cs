using System.Collections.Generic;
using Game.Battle.CombatGroups;
using Game.Battle.CombatObjects;
using Game.Battle.Reporting;
using Game.Data;
using Game.Setup;
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

        Error CanWatchBattle(IPlayer player, out int roundsLeft);

        void DbLoaderAddToCombatList(ICombatGroup group, BattleManager.BattleSide side);

        void Add(ICombatGroup combatGroup, BattleManager.BattleSide battleSide, bool allowReportAccess);

        void Remove(ICombatGroup group, BattleManager.BattleSide side, ReportState state);

        bool ExecuteTurn();

        uint GetNextGroupId();

        uint GetNextCombatObjectId();

        void DbFinishedLoading();

        T GetProperty<T>(string name);

        void SetProperty(string name, object value);

        IDictionary<string, object> ListProperties();

        void DbLoadProperties(Dictionary<string, object> dbProperties);

        /// <summary>
        ///     Fired once when the battle begins
        /// </summary>
        event BattleManager.OnBattle EnterBattle;

        /// <summary>
        ///     Fired when the battle is about to end
        /// </summary>
        event BattleManager.OnBattle AboutToExitBattle;

        /// <summary>
        ///     Fired once when the battle ends
        /// </summary>
        event BattleManager.OnBattle ExitBattle;

        /// <summary>
        ///     Fired when a new round starts
        /// </summary>
        event BattleManager.OnRound EnterRound;

        /// <summary>
        ///     Fired everytime a unit exits its turn
        /// </summary>
        event BattleManager.OnTurn ExitTurn;

        /// <summary>
        ///     Fired when a new attacker joins the battle
        /// </summary>
        event BattleManager.OnReinforce ReinforceAttacker;

        /// <summary>
        ///     Fired when a new defender joins the battle
        /// </summary>
        event BattleManager.OnReinforce ReinforceDefender;

        /// <summary>
        ///     Fired when an attacker withdraws from the battle
        /// </summary>
        event BattleManager.OnWithdraw WithdrawAttacker;

        /// <summary>
        ///     Fired when a defender withdraws from the battle
        /// </summary>
        event BattleManager.OnWithdraw WithdrawDefender;

        /// <summary>
        ///     Fired when all of the units in a group are killed
        /// </summary>
        event BattleManager.OnWithdraw GroupKilled;

        /// <summary>
        ///     Fired when one of the groups in battle receives a new unit
        /// </summary>
        event BattleManager.OnUnitUpdate GroupUnitAdded;

        /// <summary>
        ///     Fired when one of the groups in battle loses a unit
        /// </summary>
        event BattleManager.OnUnitUpdate GroupUnitRemoved;

        /// <summary>
        ///     Fired when the whole object is killed
        /// </summary>
        event BattleManager.OnUnitUpdate UnitKilled;

        /// <summary>
        ///     Fired when a single unit is killed
        /// </summary>
        event BattleManager.OnUnitCountChange UnitCountDecreased;

        /// <summary>
        ///     Fired when an attacker is unable to take his turn
        /// </summary>
        event BattleManager.OnUnitUpdate SkippedAttacker;

        /// <summary>
        ///     Fired when a unit hits another one
        /// </summary>
        event BattleManager.OnAttack ActionAttacked;
    }
}