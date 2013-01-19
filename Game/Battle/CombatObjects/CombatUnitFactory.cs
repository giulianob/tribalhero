#region

using System.Linq;
using Game.Data;
using Game.Data.BarbarianTribe;
using Game.Data.Stats;
using Game.Data.Stronghold;
using Game.Data.Troop;
using Game.Logic.Actions;
using Game.Logic.Formulas;
using Game.Setup;
using Ninject;

#endregion

namespace Game.Battle.CombatObjects
{
    public class CombatUnitFactory : ICombatUnitFactory
    {
        private readonly IKernel kernel;

        private readonly ObjectTypeFactory objectTypeFactory;

        public CombatUnitFactory(IKernel kernel, ObjectTypeFactory objectTypeFactory)
        {
            this.kernel = kernel;
            this.objectTypeFactory = objectTypeFactory;
        }

        public CombatStructure CreateStructureCombatUnit(IBattleManager battleManager, IStructure structure)
        {
            return new CombatStructure(battleManager.GetNextCombatObjectId(),
                                       battleManager.BattleId,
                                       structure,
                                       kernel.Get<BattleFormulas>().LoadStats(structure),
                                       kernel.Get<Formula>(),
                                       kernel.Get<IActionFactory>(),
                                       kernel.Get<BattleFormulas>());
        }

        public AttackCombatUnit[] CreateAttackCombatUnit(IBattleManager battleManager,
                                                         ITroopObject troop,
                                                         FormationType formation,
                                                         ushort type,
                                                         ushort count)
        {
            BaseUnitStats template = troop.City.Template[type];
            BattleStats stats = troop.Stub.Template[type];
            var groupSize = (from effect in troop.City.Technologies.GetEffects(EffectCode.UnitStatMod)
                             where
                                     ((string)effect.Value[0]).ToLower() == "groupsize" &&
                                     BattleFormulas.Current.UnitStatModCheck(stats.Base,
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
                                                                kernel.Get<BattleFormulas>());

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
            BaseUnitStats template = stub.City.Template[type];
            BattleStats stats = stub.Template[type];
            var groupSize = (from effect in stub.City.Technologies.GetEffects(EffectCode.UnitStatMod)
                             where
                                     ((string)effect.Value[0]).ToLower() == "groupsize" &&
                                     BattleFormulas.Current.UnitStatModCheck(stats.Base,
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
                                                                  kernel.Get<BattleFormulas>());
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
                                                                        kernel.Get<BattleFormulas>(),
                                                                        kernel.Get<Formula>());

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
                BarbarianTribeCombatUnit newUnit = new BarbarianTribeCombatUnit(battleManager.GetNextCombatObjectId(),
                                                                                battleManager.BattleId,
                                                                                type,
                                                                                level,
                                                                                size,
                                                                                barbarianTribe,
                                                                                kernel.Get<UnitFactory>(),
                                                                                kernel.Get<BattleFormulas>(),
                                                                                kernel.Get<Formula>());

                units[i++] = newUnit;
                count -= size;
            }
            while (count > 0);
            return units;
        }

        public StrongholdCombatStructure CreateStrongholdGateStructure(IBattleManager battleManager,
                                                                       IStronghold stronghold,
                                                                       decimal hp)
        {
            return new StrongholdCombatGate(battleManager.GetNextCombatObjectId(),
                                            battleManager.BattleId,
                                            objectTypeFactory.GetTypes("StrongholdGateStructureType")[0],
                                            stronghold.Lvl,
                                            hp,
                                            stronghold,
                                            kernel.Get<StructureFactory>(),
                                            kernel.Get<BattleFormulas>());
        }
    }
}