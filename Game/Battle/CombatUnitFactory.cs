#region

using Game.Data.Stats;
using Game.Data.Troop;
using Game.Data;
using System.Linq;
using Game.Logic.Actions;
using Game.Logic.Formulas;
using Game.Setup;
using Ninject;
using Ninject.Parameters;

#endregion

namespace Game.Battle
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
            return new CombatStructure(battleManager.BattleId, structure, kernel.Get<BattleFormulas>().LoadStats(structure), kernel.Get<Formula>(), kernel.Get<BattleFormulas>(), kernel.Get<IActionFactory>());
        }

        public AttackCombatUnit[] CreateAttackCombatUnit(IBattleManager battleManager, ITroopObject troop, FormationType formation, ushort type, ushort count)
        {
            BaseUnitStats template = troop.City.Template[type];
            BattleStats stats = troop.Stub.Template[type];
            var groupSize = (from effect in troop.City.Technologies.GetEffects(EffectCode.UnitStatMod)
                             where
                                     ((string)effect.Value[0]).ToLower() == "groupsize" &&
                                     BattleFormulas.Current.UnitStatModCheck(stats.Base, TroopBattleGroup.Attack, (string)effect.Value[3])
                             select (int)effect.Value[2]).DefaultIfEmpty<int>(0).Max() + stats.Base.GroupSize;
            
            var units = new AttackCombatUnit[(count - 1) / groupSize + 1];

            int i = 0;
            do
            {
                ushort size = (ushort)(groupSize > count ? count : groupSize);

                AttackCombatUnit newUnit = new AttackCombatUnit(battleManager.BattleId,
                                                                troop.Stub,
                                                                formation,
                                                                type,
                                                                template.Lvl,
                                                                size,
                                                                kernel.Get<UnitFactory>());                    

                units[i++] = newUnit;
                count -= size;
            } while (count > 0);
            return units;
        }

        public DefenseCombatUnit[] CreateDefenseCombatUnit(IBattleManager battleManager, ITroopStub stub, FormationType formation, ushort type, ushort count)
        {
            BaseUnitStats template = stub.City.Template[type];
            BattleStats stats = stub.Template[type];
            var groupSize = (from effect in stub.City.Technologies.GetEffects(EffectCode.UnitStatMod)
                        where ((string)effect.Value[0]).ToLower()=="groupsize" &&
                            BattleFormulas.Current.UnitStatModCheck(stats.Base, TroopBattleGroup.Defense, (string)effect.Value[3])
                        select (int)effect.Value[2]).DefaultIfEmpty().Max()+stats.Base.GroupSize;

            var units = new DefenseCombatUnit[(count - 1) / groupSize + 1];
            int i = 0;
            do
            {
                ushort size = (ushort)(groupSize > count ? count : groupSize);
                DefenseCombatUnit newUnit = new DefenseCombatUnit(battleManager.BattleId,
                                                                stub,
                                                                formation,
                                                                type,
                                                                template.Lvl,
                                                                size);
                units[i++] = newUnit;
                count -= size;
            } while (count > 0);
            return units;
        }
    }
}