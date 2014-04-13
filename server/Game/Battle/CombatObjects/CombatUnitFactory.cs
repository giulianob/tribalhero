#region

using System.Linq;
using Game.Data;
using Game.Data.BarbarianTribe;
using Game.Data.Stats;
using Game.Data.Stronghold;
using Game.Data.Troop;
using Game.Logic.Actions;
using Game.Logic.Formulas;
using Game.Map;
using Game.Setup;
using Game.Setup.DependencyInjection;
using Persistance;

#endregion

namespace Game.Battle.CombatObjects
{
    public class CombatUnitFactory : ICombatUnitFactory
    {
        private readonly IKernel kernel;

        public CombatUnitFactory(IKernel kernel)
        {
            this.kernel = kernel;
        }

        public CombatStructure CreateStructureCombatUnit(IBattleManager battleManager, IStructure structure)
        {
            return new CombatStructure(battleManager.GetNextCombatObjectId(),
                                       battleManager.BattleId,
                                       structure,
                                       kernel.Get<IBattleFormulas>().LoadStats(structure),
                                       kernel.Get<Formula>(),
                                       kernel.Get<IActionFactory>(),
                                       kernel.Get<IBattleFormulas>(),
                                       kernel.Get<ITileLocator>(),
                                       kernel.Get<IRegionManager>(),
                                       kernel.Get<IDbManager>());
        }

        public AttackCombatUnit[] CreateAttackCombatUnit(IBattleManager battleManager,
                                                         ITroopObject troop,
                                                         FormationType formation,
                                                         ushort type,
                                                         ushort count)
        {
            var battleFormulas = kernel.Get<IBattleFormulas>();

            IBaseUnitStats template = troop.City.Template[type];
            BattleStats stats = troop.Stub.Template[type];
            var groupSize = (from effect in troop.City.Technologies.GetEffects(EffectCode.UnitStatMod)
                             where
                                     ((string)effect.Value[0]).ToLower() == "groupsize" &&
                                     battleFormulas.UnitStatModCheck(stats.Base,
                                                                             TroopBattleGroup.Attack,
                                                                             (string)effect.Value[3])
                             select (int)effect.Value[2]).DefaultIfEmpty<int>(0).Max() + stats.Base.GroupSize;

            var units = new AttackCombatUnit[(count - 1) / groupSize + 1];

            int i = 0;
            do
            {
                ushort size = (ushort)(groupSize > count ? count : groupSize);

                AttackCombatUnit newUnit = new AttackCombatUnit(battleManager.GetNextCombatObjectId(),
                                                                battleManager.BattleId,
                                                                troop,
                                                                formation,
                                                                type,
                                                                template.Lvl,
                                                                size,
                                                                kernel.Get<UnitFactory>(),
                                                                battleFormulas,
                                                                kernel.Get<Formula>(),
                                                                kernel.Get<ITileLocator>(),
                                                                kernel.Get<IDbManager>());

                units[i++] = newUnit;
                count -= size;
            }
            while (count > 0);

            return units;
        }

        public DefenseCombatUnit[] CreateDefenseCombatUnit(IBattleManager battleManager,
                                                           ITroopStub stub,
                                                           FormationType formation,
                                                           ushort type,
                                                           ushort count)
        {
            var battleFormulas = kernel.Get<IBattleFormulas>();
            var formula = kernel.Get<Formula>();
            var unitFactory = kernel.Get<UnitFactory>();
            var dbManager = kernel.Get<IDbManager>();

            IBaseUnitStats template = stub.City.Template[type];
            BattleStats stats = stub.Template[type];
            var groupSize = (from effect in stub.City.Technologies.GetEffects(EffectCode.UnitStatMod)
                             where
                                     ((string)effect.Value[0]).ToLower() == "groupsize" &&
                                     battleFormulas.UnitStatModCheck(stats.Base,
                                                                     TroopBattleGroup.Defense,
                                                                     (string)effect.Value[3])
                             select (int)effect.Value[2]).DefaultIfEmpty().Max() + stats.Base.GroupSize;

            var units = new DefenseCombatUnit[(count - 1) / groupSize + 1];
            int i = 0;
            do
            {
                ushort size = (ushort)(groupSize > count ? count : groupSize);

                DefenseCombatUnit newUnit = new DefenseCombatUnit(battleManager.GetNextCombatObjectId(),
                                                                  battleManager.BattleId,
                                                                  stub,
                                                                  formation,
                                                                  type,
                                                                  template.Lvl,
                                                                  size,
                                                                  battleFormulas,
                                                                  formula,
                                                                  unitFactory,
                                                                  dbManager);
                units[i++] = newUnit;
                count -= size;
            }
            while (count > 0);

            return units;
        }

        public StrongholdCombatUnit[] CreateStrongholdCombatUnit(IBattleManager battleManager,
                                                                 IStronghold stronghold,
                                                                 ushort type,
                                                                 byte level,
                                                                 ushort count)
        {
            var groupSize = kernel.Get<UnitFactory>().GetUnitStats(type, level).Battle.GroupSize;
            var units = new StrongholdCombatUnit[(count - 1) / groupSize + 1];
            int i = 0;
            do
            {
                ushort size = (groupSize > count ? count : groupSize);
                StrongholdCombatUnit newUnit = new StrongholdCombatUnit(battleManager.GetNextCombatObjectId(),
                                                                        battleManager.BattleId,
                                                                        type,
                                                                        level,
                                                                        size,
                                                                        stronghold,
                                                                        kernel.Get<UnitFactory>(),
                                                                        kernel.Get<IBattleFormulas>(),
                                                                        kernel.Get<Formula>(),
                                                                        kernel.Get<IDbManager>());

                units[i++] = newUnit;
                count -= size;
            }
            while (count > 0);
            return units;
        }

        public BarbarianTribeCombatUnit[] CreateBarbarianTribeCombatUnit(IBattleManager battleManager,
                                                                 IBarbarianTribe barbarianTribe,
                                                                 ushort type,
                                                                 byte level,
                                                                 ushort count)
        {
            var groupSize = kernel.Get<UnitFactory>().GetUnitStats(type, level).Battle.GroupSize;
            var units = new BarbarianTribeCombatUnit[(count - 1) / groupSize + 1];
            int i = 0;
            do
            {
                ushort size = (groupSize > count ? count : groupSize);

                var newUnit = CreateBarbarianTribeCombatUnit(battleManager.GetNextCombatObjectId(),
                                                             battleManager.BattleId,
                                                             type,
                                                             level,
                                                             size,
                                                             barbarianTribe);

                units[i++] = newUnit;
                count -= size;
            }
            while (count > 0);
            return units;
        }

        public BarbarianTribeCombatUnit CreateBarbarianTribeCombatUnit(uint id,
                                                                       uint battleId,
                                                                       ushort type,
                                                                       byte level,
                                                                       ushort count,
                                                                       IBarbarianTribe barbarianTribe)
        {
            var unitFactory = kernel.Get<UnitFactory>();

            return new BarbarianTribeCombatUnit(id,
                                                battleId,
                                                type,
                                                level,
                                                count,
                                                unitFactory.GetUnitStats(type, level),
                                                barbarianTribe,
                                                kernel.Get<IBattleFormulas>(),
                                                kernel.Get<Formula>(),
                                                kernel.Get<IDbManager>());
        }

        public StrongholdCombatStructure CreateStrongholdGateStructure(IBattleManager battleManager,
                                                                       IStronghold stronghold,
                                                                       decimal hp)
        {
            var objectTypeFactory = kernel.Get<ObjectTypeFactory>();

            return new StrongholdCombatGate(battleManager.GetNextCombatObjectId(),
                                            battleManager.BattleId,
                                            (ushort)objectTypeFactory.GetTypes("StrongholdGateStructureType")[0],
                                            stronghold.Lvl,
                                            hp,
                                            stronghold,
                                            kernel.Get<IStructureCsvFactory>(),
                                            kernel.Get<IBattleFormulas>(),
                                            kernel.Get<IDbManager>());
        }
    }
}