#region

using System;
using System.Collections.Generic;
using Game.Battle;
using Game.Battle.CombatGroups;
using Game.Battle.CombatObjects;
using Game.Data;
using Game.Data.Troop;
using System.Linq;
using Game.Logic.Actions;
using Game.Map;
using Game.Setup;

#endregion

namespace Game.Logic.Procedures
{
    public class BattleProcedure
    {
        private readonly ICombatUnitFactory combatUnitFactory;

        private readonly ICombatGroupFactory combatGroupFactory;

        private readonly RadiusLocator radiusLocator;

        private readonly IBattleManagerFactory battleManagerFactory;

        private readonly IActionFactory actionFactory;

        public BattleProcedure(ICombatUnitFactory combatUnitFactory, ICombatGroupFactory combatGroupFactory, RadiusLocator radiusLocator, IBattleManagerFactory battleManagerFactory, IActionFactory actionFactory)
        {
            this.combatUnitFactory = combatUnitFactory;
            this.combatGroupFactory = combatGroupFactory;
            this.radiusLocator = radiusLocator;
            this.battleManagerFactory = battleManagerFactory;
            this.actionFactory = actionFactory;
        }

        public virtual void JoinOrCreateBattle(ICity targetCity, ITroopObject attackerTroopObject, out uint groupId)
        {
            var stub = attackerTroopObject.Stub;
            // If battle already exists, then we just join it in also bringing any new units
            if (targetCity.Battle != null)
            {
                AddLocalUnitsToBattle(targetCity.Battle, targetCity);
                AddLocalStructuresToBattle(targetCity.Battle, targetCity, stub);
                groupId = AddAttackerToBattle(targetCity.Battle, attackerTroopObject);
            }
            // Otherwise, the battle has to be created
            else
            {
                targetCity.Battle = battleManagerFactory.CreateBattleManager(new BattleLocation(BattleLocationType.City, targetCity.Id),
                                                                             new BattleOwner(BattleOwnerType.City, targetCity.Id),
                                                                             targetCity);

                var battlePassiveAction = actionFactory.CreateBattlePassiveAction(targetCity.City.Id);

                AddLocalStructuresToBattle(targetCity.Battle, targetCity, stub);
                groupId = AddAttackerToBattle(targetCity.Battle, attackerTroopObject);

                Error result = targetCity.Worker.DoPassive(targetCity, battlePassiveAction, false);
                if (result != Error.Ok)
                {
                    throw new Exception(string.Format("Failed to start a battle due to error {0}", result));
                }
            }            
        }

        private IEnumerable<IStructure> GetStructuresInRadius(IEnumerable<IStructure> structures, ITroopObject troopObject)
        {
            Location troopLocation = new Location(troopObject.X, troopObject.Y);

            return
                    structures.Where(
                                     structure =>
                                     radiusLocator.IsOverlapping(troopLocation,
                                                                 troopObject.Stats.AttackRadius,
                                                                 new Location(structure.X, structure.Y),
                                                                 structure.Stats.Base.Radius));
        }

        public virtual void MoveUnitFormation(ITroopStub stub, FormationType source, FormationType target)
        {
            stub[target].Add(stub[source]);
            stub[source].Clear();
        }

        public virtual void AddLocalStructuresToBattle(IBattleManager battleManager, ICity targetCity, ITroopStub attackerStub)
        {
            var localGroup = GetOrCreateLocalGroup(targetCity.Battle, targetCity);
            foreach (IStructure structure in GetStructuresInRadius(targetCity, attackerStub.TroopObject))
            {
                localGroup.Add(combatUnitFactory.CreateStructureCombatUnit(battleManager, structure));
            }
        }
        
        public virtual void AddLocalUnitsToBattle(IBattleManager battleManager, ICity city)
        {
            if (city.DefaultTroop[FormationType.Normal].Count == 0)
            {
                return;
            }

            city.DefaultTroop.BeginUpdate();
            city.DefaultTroop.State = TroopState.Battle;
            city.DefaultTroop.Template.LoadStats(TroopBattleGroup.Local);
            MoveUnitFormation(city.DefaultTroop, FormationType.Normal, FormationType.InBattle);            
            city.DefaultTroop.EndUpdate();

            // Add to local group
            var combatGroup = GetOrCreateLocalGroup(battleManager, city);
            foreach (var kvp in city.DefaultTroop[FormationType.Normal])
            {
                combatUnitFactory.CreateDefenseCombatUnit(battleManager, city.DefaultTroop, FormationType.InBattle, kvp.Key, kvp.Value).ToList().ForEach(combatGroup.Add);
            }
        }
        
        public virtual uint AddAttackerToBattle(IBattleManager battleManager, ITroopObject troopObject)
        {
            var defensiveGroup = combatGroupFactory.CreateCityOffensiveCombatGroup(battleManager, battleManager.GetNextGroupId(), troopObject);
            foreach (var kvp in troopObject.Stub.SelectMany(formation => formation))
            {
                combatUnitFactory.CreateAttackCombatUnit(battleManager, troopObject, FormationType.Defense, kvp.Key, kvp.Value).ToList().ForEach(defensiveGroup.Add);
            }
            battleManager.Add(defensiveGroup, BattleManager.BattleSide.Attack);

            return defensiveGroup.Id;
        }

        public virtual void AddReinforcementToBattle(IBattleManager battleManager, ITroopStub stub)
        {
            var defensiveGroup = combatGroupFactory.CreateCityDefensiveCombatGroup(battleManager, battleManager.GetNextGroupId(), stub);
            foreach (var kvp in stub.SelectMany(formation => formation))
            {
                combatUnitFactory.CreateDefenseCombatUnit(battleManager, stub, FormationType.Defense, kvp.Key, kvp.Value).ToList().ForEach(defensiveGroup.Add);
            }
            battleManager.Add(defensiveGroup, BattleManager.BattleSide.Defense);
        }

        private CombatGroup GetOrCreateLocalGroup(IBattleManager battleManager, ICity city)
        {
            var combatGroup = battleManager.GetCombatGroup(1);
            if (combatGroup == null)
            {                     
                combatGroup = combatGroupFactory.CreateCityDefensiveCombatGroup(battleManager, 1, city.DefaultTroop);
                battleManager.Add(combatGroup, BattleManager.BattleSide.Defense);
            }

            return combatGroup;
        }

        /// <summary>
        /// Repairs all structures up to max HP but depends on percentage from sense of urgency effect
        /// </summary>
        /// <param name="city"></param>
        /// <param name="maxHp"></param>
        internal virtual void SenseOfUrgency(ICity city, uint maxHp)
        {
            // Prevent overflow, just to be safe
            maxHp = Math.Min(50000, maxHp);

            int healPercent = Math.Min(100, city.Technologies.GetEffects(EffectCode.SenseOfUrgency).Sum(x => (int)x.Value[0]));

            if (healPercent == 0)
                return;

            ushort restore = (ushort)(maxHp * (healPercent / 100f));

            foreach (IStructure structure in city) {
                if (structure.State.Type == ObjectState.Battle || structure.Stats.Hp == structure.Stats.Base.Battle.MaxHp)
                    continue;

                structure.BeginUpdate();
                structure.Stats.Hp += restore;
                structure.EndUpdate();
            }
        }
    }
}