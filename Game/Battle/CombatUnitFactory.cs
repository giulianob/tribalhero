#region

using Game.Data.Stats;
using Game.Data.Troop;
using Game.Data;
using System.Linq;
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

        public CombatStructure CreateStructureCombatUnit(IBattleManager battleManager, Structure structure)
        {
            return kernel.Get<CombatStructure>(new ConstructorArgument("owner", battleManager),
                                               new ConstructorArgument("structure", structure),
                                               new ConstructorArgument("stats", BattleFormulas.LoadStats(structure)));
        }

        public AttackCombatUnit[] CreateAttackCombatUnit(IBattleManager owner, TroopObject troop, FormationType formation, ushort type, ushort count)
        {
            BaseUnitStats template = troop.City.Template[type];
            BattleStats stats = troop.Stub.Template[type];
            var groupSize = (from effect in troop.City.Technologies.GetEffects(EffectCode.UnitStatMod, EffectInheritance.All)
                         where ((string)effect.Value[0]).ToLower()=="groupsize" &&
                               BattleFormulas.UnitStatModCheck(stats.Base, TroopBattleGroup.Attack, (string)effect.Value[3])
                         select (int)effect.Value[2]).DefaultIfEmpty<int>(0).Max() + stats.Base.GroupSize;
            
            var units = new AttackCombatUnit[(count - 1) / groupSize + 1];

            int i = 0;
            do
            {
                ushort size = (ushort)(groupSize > count ? count : groupSize);

                AttackCombatUnit newUnit = kernel.Get<AttackCombatUnit>(new ConstructorArgument("owner", owner),
                                                                        new ConstructorArgument("stub", troop.Stub),
                                                                        new ConstructorArgument("formation", formation),
                                                                        new ConstructorArgument("type", type),
                                                                        new ConstructorArgument("lvl", template.Lvl),
                                                                        new ConstructorArgument("count", size));

                units[i++] = newUnit;
                count -= size;
            } while (count > 0);
            return units;
        }

        public DefenseCombatUnit[] CreateDefenseCombatUnit(IBattleManager owner, TroopStub stub, FormationType formation, ushort type, ushort count)
        {
            BaseUnitStats template = stub.City.Template[type];
            BattleStats stats = stub.Template[type];
            var groupSize = (from effect in stub.City.Technologies.GetEffects(EffectCode.UnitStatMod, EffectInheritance.All)
                        where ((string)effect.Value[0]).ToLower()=="groupsize" &&
                            BattleFormulas.UnitStatModCheck(stats.Base, TroopBattleGroup.Defense, (string)effect.Value[3])
                        select (int)effect.Value[2]).DefaultIfEmpty().Max()+stats.Base.GroupSize;

            var units = new DefenseCombatUnit[(count - 1) / groupSize + 1];
            int i = 0;
            do
            {
                ushort size = (ushort)(groupSize > count ? count : groupSize);
                DefenseCombatUnit newUnit = kernel.Get<DefenseCombatUnit>(new ConstructorArgument("owner", owner),
                                                                          new ConstructorArgument("stub", stub),
                                                                          new ConstructorArgument("formation", formation),
                                                                          new ConstructorArgument("type", type),
                                                                          new ConstructorArgument("lvl", template.Lvl),
                                                                          new ConstructorArgument("count", size));                
                units[i++] = newUnit;
                count -= size;
            } while (count > 0);
            return units;
        }
    }
}