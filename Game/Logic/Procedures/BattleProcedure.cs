#region

using System;
using System.Collections.Generic;
using System.Linq;
using Game.Battle;
using Game.Battle.CombatGroups;
using Game.Battle.CombatObjects;
using Game.Data;
using Game.Data.Stronghold;
using Game.Data.Troop;
using Game.Logic.Actions;
using Game.Logic.Formulas;
using Game.Map;
using Game.Setup;
using Game.Util;

#endregion

namespace Game.Logic.Procedures
{
    public class BattleProcedure
    {
        private readonly IActionFactory actionFactory;

        private readonly IBattleManagerFactory battleManagerFactory;

        private readonly ICombatGroupFactory combatGroupFactory;

        private readonly ICombatUnitFactory combatUnitFactory;

        private readonly Formula formula;

        private readonly ObjectTypeFactory objectTypeFactory;

        private readonly RadiusLocator radiusLocator;

        [Obsolete("For testing only", true)]
        protected BattleProcedure()
        {
        }

        public BattleProcedure(ICombatUnitFactory combatUnitFactory,
                               ICombatGroupFactory combatGroupFactory,
                               RadiusLocator radiusLocator,
                               IBattleManagerFactory battleManagerFactory,
                               IActionFactory actionFactory,
                               ObjectTypeFactory objectTypeFactory,
                               Formula formula)
        {
            this.combatUnitFactory = combatUnitFactory;
            this.combatGroupFactory = combatGroupFactory;
            this.radiusLocator = radiusLocator;
            this.battleManagerFactory = battleManagerFactory;
            this.actionFactory = actionFactory;
            this.objectTypeFactory = objectTypeFactory;
            this.formula = formula;
        }

        public virtual void JoinOrCreateCityBattle(ICity targetCity,
                                                   ITroopObject attackerTroopObject,
                                                   out ICombatGroup combatGroup,
                                                   out uint battleId)
        {
            // If battle already exists, then we just join it in also bringing any new units
            if (targetCity.Battle != null)
            {
                AddLocalUnitsToBattle(targetCity.Battle, targetCity);
                AddLocalStructuresToBattle(targetCity.Battle, targetCity, attackerTroopObject);
                combatGroup = AddAttackerToBattle(targetCity.Battle, attackerTroopObject);
            }
                    // Otherwise, the battle has to be created
            else
            {
                targetCity.Battle =
                        battleManagerFactory.CreateBattleManager(
                                                                 new BattleLocation(BattleLocationType.City,
                                                                                    targetCity.Id),
                                                                 new BattleOwner(BattleOwnerType.City, targetCity.Id),
                                                                 targetCity);

                var battlePassiveAction = actionFactory.CreateCityBattlePassiveAction(targetCity.Id);

                AddLocalStructuresToBattle(targetCity.Battle, targetCity, attackerTroopObject);
                combatGroup = AddAttackerToBattle(targetCity.Battle, attackerTroopObject);

                Error result = targetCity.Worker.DoPassive(targetCity, battlePassiveAction, false);
                if (result != Error.Ok)
                {
                    throw new Exception(string.Format("Failed to start a battle due to error {0}", result));
                }
            }

            battleId = targetCity.Battle.BattleId;
        }

        private IEnumerable<IStructure> GetStructuresInRadius(IEnumerable<IStructure> structures,
                                                              ITroopObject troopObject)
        {
            Position troopPosition = new Position(troopObject.X, troopObject.Y);

            return
                    structures.Where(
                                     structure =>
                                     radiusLocator.IsOverlapping(troopPosition,
                                                                 troopObject.Stats.AttackRadius,
                                                                 new Position(structure.X, structure.Y),
                                                                 structure.Stats.Base.Radius));
        }

        // TODO: Change to instance method.. need to make sure City is being created by ninject first though
        public static bool IsNewbieProtected(IPlayer player)
        {
            return SystemClock.Now.Subtract(player.Created).TotalSeconds < Config.newbie_protection;
        }

        public virtual Error CanCityBeAttacked(ICity attackerCity, ICity targetCity)
        {
            // Can't attack tribes mate
            if (attackerCity.Owner.Tribesman != null && targetCity.Owner.Tribesman != null &&
                attackerCity.Owner.Tribesman.Tribe == targetCity.Owner.Tribesman.Tribe)
            {
                return Error.AssignmentCantAttackFriend;
            }

            // Can't attack if target is under newbie protection
            if (IsNewbieProtected(targetCity.Owner))
            {
                return Error.PlayerNewbieProtection;
            }

            // Can't attack cities that are being deleted
            if (targetCity.Deleted != City.DeletedState.NotDeleted)
            {
                return Error.ObjectNotAttackable;
            }

            return Error.Ok;
        }

        public virtual Error CanStructureBeAttacked(IStructure structure)
        {
            // Can't attack structures that are being built
            if (structure.Lvl == 0)
            {
                return Error.ObjectNotAttackable;
            }

            // Can't attack structures that are marked as Unattackable
            if (objectTypeFactory.IsStructureType("Unattackable", structure))
            {
                return Error.ObjectNotAttackable;
            }

            // Can't attack understroyabled structure that are level 1
            if ((objectTypeFactory.IsStructureType("Undestroyable", structure) && structure.Lvl <= 1))
            {
                return Error.StructureUndestroyable;
            }

            return Error.Ok;
        }

        public virtual void MoveUnitFormation(ITroopStub stub, FormationType source, FormationType target)
        {
            stub[target].Add(stub[source]);
            stub[source].Clear();
        }

        protected virtual void AddLocalStructuresToBattle(IBattleManager battleManager,
                                                          ICity targetCity,
                                                          ITroopObject attackerTroopObject)
        {
            var localGroup = GetOrCreateLocalGroup(targetCity.Battle, targetCity);
            foreach (IStructure structure in
                    GetStructuresInRadius(targetCity, attackerTroopObject)
                            .Where(
                                   structure =>
                                   !structure.IsBlocked && structure.Stats.Hp > 0 && structure.State.Type == ObjectState.Normal &&
                                   CanStructureBeAttacked(structure) == Error.Ok))
            {
                structure.BeginUpdate();
                structure.State = GameObjectState.BattleState(battleManager.BattleId);
                structure.EndUpdate();

                localGroup.Add(combatUnitFactory.CreateStructureCombatUnit(battleManager, structure));
            }
        }

        public virtual void AddLocalUnitsToBattle(IBattleManager battleManager, ICity city)
        {
            if (city.DefaultTroop[FormationType.Normal].Count == 0)
            {
                return;
            }

            // Move to in battle formation
            var unitsToJoinBattle = city.DefaultTroop[FormationType.Normal].ToList();
            city.DefaultTroop.BeginUpdate();
            city.DefaultTroop.State = TroopState.Battle;
            city.DefaultTroop.Template.LoadStats(TroopBattleGroup.Local);
            MoveUnitFormation(city.DefaultTroop, FormationType.Normal, FormationType.InBattle);
            city.DefaultTroop.EndUpdate();

            // Add to local group
            var combatGroup = GetOrCreateLocalGroup(battleManager, city);
            foreach (KeyValuePair<ushort, ushort> kvp in unitsToJoinBattle)
            {
                var defenseCombatUnits = combatUnitFactory.CreateDefenseCombatUnit(battleManager, city.DefaultTroop, FormationType.InBattle, kvp.Key, kvp.Value);
                defenseCombatUnits.ToList().ForEach(combatGroup.Add);
            }
        }

        protected virtual ICombatGroup AddAttackerToBattle(IBattleManager battleManager, ITroopObject troopObject)
        {
            var offensiveGroup = combatGroupFactory.CreateCityOffensiveCombatGroup(battleManager.BattleId,
                                                                                   battleManager.GetNextGroupId(),
                                                                                   troopObject);
            foreach (
                    var attackCombatUnits in
                            troopObject.Stub.SelectMany(formation => formation)
                                       .Select(
                                               kvp =>
                                               combatUnitFactory.CreateAttackCombatUnit(battleManager,
                                                                                        troopObject,
                                                                                        FormationType.Attack,
                                                                                        kvp.Key,
                                                                                        kvp.Value)))
            {
                attackCombatUnits.ToList().ForEach(offensiveGroup.Add);
            }

            battleManager.Add(offensiveGroup, BattleManager.BattleSide.Attack, true);

            return offensiveGroup;
        }

        public virtual uint AddReinforcementToBattle(IBattleManager battleManager, ITroopStub stub, FormationType formationToAddToBattle)
        {
            stub.BeginUpdate();
            stub.Template.LoadStats(TroopBattleGroup.Defense);
            stub.EndUpdate();

            var defensiveGroup = combatGroupFactory.CreateCityDefensiveCombatGroup(battleManager.BattleId,
                                                                                   battleManager.GetNextGroupId(),
                                                                                   stub);
            foreach (var kvp in stub[formationToAddToBattle])
            {
                combatUnitFactory.CreateDefenseCombatUnit(battleManager, stub, formationToAddToBattle, kvp.Key, kvp.Value)
                                 .ToList()
                                 .ForEach(defensiveGroup.Add);
            }
            battleManager.Add(defensiveGroup, BattleManager.BattleSide.Defense, true);

            return defensiveGroup.Id;
        }

        private ICombatGroup GetOrCreateLocalGroup(IBattleManager battleManager, ICity city)
        {
            var combatGroup = battleManager.GetCombatGroup(1);
            if (combatGroup == null)
            {
                combatGroup = combatGroupFactory.CreateCityDefensiveCombatGroup(battleManager.BattleId,
                                                                                1,
                                                                                city.DefaultTroop);
                battleManager.Add(combatGroup, BattleManager.BattleSide.Defense, false);
            }

            return combatGroup;
        }

        /// <summary>
        ///     Repairs all structures up to max HP but depends on percentage from sense of urgency effect
        /// </summary>
        /// <param name="city"></param>
        /// <param name="maxHp"></param>
        internal virtual void SenseOfUrgency(ICity city, uint maxHp)
        {
            // Prevent overflow, just to be safe
            maxHp = Math.Min(50000, maxHp);

            int healPercent = Math.Min(100,
                                       city.Technologies.GetEffects(EffectCode.SenseOfUrgency).Sum(x => (int)x.Value[0]));

            if (healPercent == 0)
            {
                return;
            }

            ushort restore = (ushort)(maxHp * (healPercent / 100f));

            foreach (
                    IStructure structure in
                            city.Where(
                                       structure =>
                                       structure.State.Type != ObjectState.Battle &&
                                       structure.Stats.Hp != structure.Stats.Base.Battle.MaxHp))
            {
                structure.BeginUpdate();
                structure.Stats.Hp += restore;
                structure.EndUpdate();
            }
        }

        public virtual bool HasTooManyAttacks(ICity city)
        {
            return city.Worker.PassiveActions.Values.Count(action => action.Category == ActionCategory.Attack) > 20;
        }

        public virtual bool HasTooManyDefenses(ICity city)
        {
            return city.Worker.PassiveActions.Values.Count(action => action.Category == ActionCategory.Defense) > 20;
        }

        public virtual Error CanStrongholdBeAttacked(ICity city, IStronghold stronghold)
        {
            if (stronghold.StrongholdState == StrongholdState.Inactive)
            {
                return Error.StrongholdStillInactive;
            }

            if (!city.Owner.IsInTribe)
            {
                return Error.TribesmanNotPartOfTribe;
            }

            if (city.Owner.Tribesman.Tribe == stronghold.Tribe)
            {
                return Error.StrongholdCantAttackSelf;
            }

            if (stronghold.GateOpenTo != null && stronghold.GateOpenTo != city.Owner.Tribesman.Tribe)
            {
                return Error.StrongholdGateNotOpenToTribe;
            }

            return Error.Ok;
        }

        public virtual Error CanStrongholdBeDefended(ICity city, IStronghold stronghold)
        {
            if (stronghold.StrongholdState == StrongholdState.Inactive)
            {
                return Error.StrongholdStillInactive;
            }

            if (!city.Owner.IsInTribe)
            {
                return Error.TribesmanNotPartOfTribe;
            }

            if (city.Owner.Tribesman.Tribe != stronghold.Tribe)
            {
                return Error.StrongholdBelongsToOther;
            }

            return Error.Ok;
        }

        public virtual void JoinOrCreateStrongholdGateBattle(IStronghold targetStronghold,
                                                             ITroopObject attackerTroopObject,
                                                             out ICombatGroup combatGroup,
                                                             out uint battleId)
        {
            // If battle already exists, then we just join it in also bringing any new units
            if (targetStronghold.GateBattle != null)
            {
                combatGroup = AddAttackerToBattle(targetStronghold.GateBattle, attackerTroopObject);
            }
                    // Otherwise, the battle has to be created
            else
            {
                var battleOwner = targetStronghold.Tribe == null
                                          ? new BattleOwner(BattleOwnerType.Stronghold, targetStronghold.Id)
                                          : new BattleOwner(BattleOwnerType.Tribe, targetStronghold.Tribe.Id);

                targetStronghold.GateBattle =
                        battleManagerFactory.CreateStrongholdGateBattleManager(
                                                                               new BattleLocation(
                                                                                       BattleLocationType.StrongholdGate,
                                                                                       targetStronghold.Id),
                                                                               battleOwner,
                                                                               targetStronghold);

                combatGroup = AddAttackerToBattle(targetStronghold.GateBattle, attackerTroopObject);

                var battlePassiveAction = actionFactory.CreateStrongholdGateBattlePassiveAction(targetStronghold.Id);
                Error result = targetStronghold.Worker.DoPassive(targetStronghold, battlePassiveAction, false);
                if (result != Error.Ok)
                {
                    throw new Exception(string.Format("Failed to start a battle due to error {0}", result));
                }
                targetStronghold.BeginUpdate();
                targetStronghold.State = GameObjectState.BattleState(targetStronghold.GateBattle.BattleId);
                targetStronghold.EndUpdate();
            }

            battleId = targetStronghold.GateBattle.BattleId;
        }

        public virtual ICombatGroup AddStrongholdGateToBattle(IBattleManager battle, IStronghold stronghold)
        {
            var strongholdCombatGroup = combatGroupFactory.CreateStrongholdCombatGroup(battle.BattleId,
                                                                                       battle.GetNextGroupId(),
                                                                                       stronghold);
            if (stronghold.Gate == 0)
            {
                throw new Exception("Dead gate trying to join the battle");
            }

            strongholdCombatGroup.Add(combatUnitFactory.CreateStrongholdGateStructure(battle,
                                                                                      stronghold,
                                                                                      stronghold.Gate));

            battle.Add(strongholdCombatGroup, BattleManager.BattleSide.Defense, false);

            return strongholdCombatGroup;
        }

        public virtual ICombatGroup AddStrongholdUnitsToBattle(IBattleManager battle,
                                                               IStronghold stronghold,
                                                               IEnumerable<Unit> units)
        {
            var strongholdCombatGroup = combatGroupFactory.CreateStrongholdCombatGroup(battle.BattleId,
                                                                                       battle.GetNextGroupId(),
                                                                                       stronghold);

            foreach (var unit in units)
            {
                foreach (
                        var obj in
                                combatUnitFactory.CreateStrongholdCombatUnit(battle,
                                                                             stronghold,
                                                                             unit.Type,
                                                                             (byte)Math.Max(1, stronghold.Lvl / 2),
                                                                             unit.Count))
                {
                    strongholdCombatGroup.Add(obj);
                }
            }

            battle.Add(strongholdCombatGroup, BattleManager.BattleSide.Defense, false);

            return strongholdCombatGroup;
        }

        public virtual void JoinOrCreateStrongholdMainBattle(IStronghold targetStronghold,
                                                             ITroopObject attackerTroopObject,
                                                             out ICombatGroup combatGroup,
                                                             out uint battleId)
        {
            // If battle already exists, then we just join it in also bringing any new units
            if (targetStronghold.MainBattle != null)
            {
                combatGroup = AddAttackerToBattle(targetStronghold.MainBattle, attackerTroopObject);
            }
                    // Otherwise, the battle has to be created
            else
            {
                var battleOwner = targetStronghold.Tribe == null
                                          ? new BattleOwner(BattleOwnerType.Stronghold, targetStronghold.Id)
                                          : new BattleOwner(BattleOwnerType.Tribe, targetStronghold.Tribe.Id);

                targetStronghold.MainBattle =
                        battleManagerFactory.CreateStrongholdMainBattleManager(
                                                                               new BattleLocation(
                                                                                       BattleLocationType.Stronghold,
                                                                                       targetStronghold.Id),
                                                                               battleOwner,
                                                                               targetStronghold);

                targetStronghold.MainBattle.SetProperty("defense_stronghold_meter",
                                                        formula.GetMainBattleMeter(targetStronghold.Lvl));
                targetStronghold.MainBattle.SetProperty("offense_stronghold_meter",
                                                        formula.GetMainBattleMeter(targetStronghold.Lvl));

                combatGroup = AddAttackerToBattle(targetStronghold.MainBattle, attackerTroopObject);

                var battlePassiveAction = actionFactory.CreateStrongholdMainBattlePassiveAction(targetStronghold.Id);
                Error result = targetStronghold.Worker.DoPassive(targetStronghold, battlePassiveAction, false);
                if (result != Error.Ok)
                {
                    throw new Exception(string.Format("Failed to start a battle due to error {0}", result));
                }
            }

            battleId = targetStronghold.MainBattle.BattleId;
        }
    }
}