using System;
using System.Collections.Generic;
using System.Linq;
using Game.Battle;
using Game.Battle.CombatGroups;
using Game.Battle.CombatObjects;
using Game.Data;
using Game.Data.Troop;
using Game.Logic.Actions;
using Game.Map;
using Game.Setup;

namespace Game.Logic.Procedures
{
    public class CityBattleProcedure
    {
        private readonly BattleProcedure battleProcedure;

        private readonly ICombatGroupFactory combatGroupFactory;

        private readonly ICombatUnitFactory combatUnitFactory;

        private readonly IActionFactory actionFactory;

        private readonly IBattleManagerFactory battleManagerFactory;

        private readonly ObjectTypeFactory objectTypeFactory;

        private readonly TileLocator tileLocator;

        [Obsolete("For testing only", true)]
        public CityBattleProcedure()
        {
        }

        public CityBattleProcedure(TileLocator tileLocator,
                                   IBattleManagerFactory battleManagerFactory,
                                   IActionFactory actionFactory,
                                   ObjectTypeFactory objectTypeFactory,
                                   BattleProcedure battleProcedure,
                                   ICombatGroupFactory combatGroupFactory,
                                   ICombatUnitFactory combatUnitFactory)
        {
            this.tileLocator = tileLocator;
            this.battleManagerFactory = battleManagerFactory;
            this.actionFactory = actionFactory;
            this.objectTypeFactory = objectTypeFactory;
            this.battleProcedure = battleProcedure;
            this.combatGroupFactory = combatGroupFactory;
            this.combatUnitFactory = combatUnitFactory;
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
                combatGroup = battleProcedure.AddAttackerToBattle(targetCity.Battle, attackerTroopObject);
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
                combatGroup = battleProcedure.AddAttackerToBattle(targetCity.Battle, attackerTroopObject);

                Error result = targetCity.Worker.DoPassive(targetCity, battlePassiveAction, false);
                if (result != Error.Ok)
                {
                    throw new Exception(string.Format("Failed to start a battle due to error {0}", result));
                }
            }

            battleId = targetCity.Battle.BattleId;
        }

        private IEnumerable<IStructure> GetStructuresInRadius(IEnumerable<IStructure> structures, ITroopObject troopObject)
        {
            Position troopPosition = new Position(troopObject.X, troopObject.Y);

            return
                    structures.Where(
                                     structure =>
                                     tileLocator.IsOverlapping(troopPosition,
                                                                 troopObject.Stats.AttackRadius,
                                                                 new Position(structure.X, structure.Y),
                                                                 structure.Stats.Base.Radius));
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
            if (BattleProcedure.IsNewbieProtected(targetCity.Owner))
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
            if (structure.IsBlocked != 0 || structure.Stats.Hp == 0 || structure.Lvl == 0)
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
                            .Where(structure => structure.State.Type == ObjectState.Normal && CanStructureBeAttacked(structure) == Error.Ok))
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
    }
}