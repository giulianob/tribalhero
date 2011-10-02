#region

using Game.Data.Stats;
using Game.Data.Troop;
using Game.Data;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Game.Battle
{
    public class CombatUnitFactory
    {
        public AttackCombatUnit[] CreateAttackCombatUnit(BattleManager owner, TroopObject troop, FormationType formation, ushort type, ushort count)
        {
            BaseUnitStats template = troop.City.Template[type];
            BattleStats stats = troop.Stub.Template[type];
            var groupSize = (from effect in troop.City.Technologies.GetEffects(EffectCode.UnitStatMod, EffectInheritance.All)
                         where ((string)effect.Value[0]).ToLower()=="groupsize" &&
                               BattleFormulas.UnitStatModCheck(stats.Base, TroopBattleGroup.Attack, (string)effect.Value[3])
                         select (int)effect.Value[2]).DefaultIfEmpty<int>(0).Max() + stats.Base.GroupSize;
            var units = new AttackCombatUnit[(count - 1) / groupSize + 1];
            AttackCombatUnit newUnit;
            int i = 0;
            do
            {
                ushort size = (ushort)(groupSize > count ? count : groupSize);
                newUnit = new AttackCombatUnit(owner, troop.Stub, formation, type, template.Lvl, size);
                units[i++] = newUnit;
                count -= size;
            } while (count > 0);
            return units;
        }

        public DefenseCombatUnit[] CreateDefenseCombatUnit(BattleManager owner, TroopStub stub, FormationType formation, ushort type, ushort count)
        {
            BaseUnitStats template = stub.City.Template[type];
            BattleStats stats = stub.Template[type];
            var mods = (from effect in stub.City.Technologies.GetEffects(EffectCode.UnitStatMod, EffectInheritance.All)
                        where effect.Id == EffectCode.UnitStatMod
                        where BattleFormulas.UnitStatModCheck(stats.Base, TroopBattleGroup.Defense, (string)effect.Value[3])
                        select (int)effect.Value[2]).DefaultIfEmpty().Max();

            var groupSize = mods + stats.Base.GroupSize;

            var units = new DefenseCombatUnit[(count - 1) / groupSize + 1];
            DefenseCombatUnit newUnit;
            int i = 0;
            do
            {
                ushort size = (ushort)(groupSize > count ? count : groupSize);
                newUnit = new DefenseCombatUnit(owner, stub, formation, type, template.Lvl, size);
                units[i++] = newUnit;
                count -= size;
            } while (count > 0);
            return units;
        }
    }
}