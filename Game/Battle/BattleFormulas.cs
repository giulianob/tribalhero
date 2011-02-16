#region

using System;
using System.Linq;
using Game.Data;
using Game.Data.Stats;
using Game.Data.Troop;
using Game.Setup;
using System.Collections.Generic;

#endregion

namespace Game.Battle
{
    public class BattleFormulas
    {
        public static int MissChance(bool isAttacker, CombatList defenders, CombatList attackers)
        {
            int defendersUpkeep = defenders.Sum(x => x.Upkeep);
            int attackersUpkeep = attackers.Sum(x => x.Upkeep);

            int delta = isAttacker ? Math.Max(0, attackersUpkeep - defendersUpkeep) : Math.Max(0, defendersUpkeep - attackersUpkeep);

            return Math.Min(delta*2, 25);
        }

        public static int GetUnitsPerStructure(Structure structure)
        {
            var units = new[] { 20, 20, 25, 31, 38, 44, 51, 59, 67, 76, 86, 96, 107, 119, 131, 145 };
            return units[structure.Lvl];
        }

        public static double GetArmorClassModifier(WeaponClass weapon, ArmorClass armor)
        {
            switch(weapon)
            {
                case WeaponClass.Basic:
                    switch(armor)
                    {
                        case ArmorClass.Leather:
                        case ArmorClass.Wooden:
                            return 1;
                        case ArmorClass.Metal:
                        case ArmorClass.Stone:
                            return 0.6;
                    }
                    break;
                case WeaponClass.Elemental:
                    switch(armor)
                    {
                        case ArmorClass.Leather:
                        case ArmorClass.Wooden:
                            return 0.7;
                        case ArmorClass.Metal:
                        case ArmorClass.Stone:
                            return 1.4;
                    }
                    break;
            }

            return 1;
        }

        public static double GetArmorTypeModifier(WeaponType weapon, ArmorType armor)
        {
            const double nodamage = 0.1;
            const double weakest = 0.2;
            const double weaker = 0.4;
            const double weak = 0.7;
            const double good = 1;
            const double strong = 1.4;
            const double stronger = 1.7;
            const double strongest = 2;

            switch(weapon)
            {
                case WeaponType.Sword:
                    switch(armor)
                    {
                        case ArmorType.Ground:
                            return strong;
                        case ArmorType.Mount:
                            return weak;
                        case ArmorType.Machine:
                            return strong;
                        case ArmorType.Building:
                            return weakest;
                    }
                    break;
                case WeaponType.Pike:
                    switch(armor)
                    {
                        case ArmorType.Ground:
                            return weak;
                        case ArmorType.Mount:
                            return stronger;
                        case ArmorType.Machine:
                            return weak;
                        case ArmorType.Building:
                            return weakest;
                    }
                    break;
                case WeaponType.Bow:
                    switch(armor)
                    {
                        case ArmorType.Ground:
                            return strong;
                        case ArmorType.Mount:
                            return good;
                        case ArmorType.Machine:
                            return weakest;
                        case ArmorType.Building:
                            return nodamage;
                    }
                    break;
                case WeaponType.Ball:
                    switch(armor)
                    {
                        case ArmorType.Ground:
                            return nodamage;
                        case ArmorType.Mount:
                            return nodamage;
                        case ArmorType.Machine:
                            return weak;
                        case ArmorType.Building:
                            return strongest;
                    }
                    break;
                case WeaponType.Barricade:
                    switch(armor)
                    {
                        case ArmorType.Ground:
                            return weaker;
                        case ArmorType.Mount:
                            return weaker;
                        case ArmorType.Machine:
                            return weaker;
                        case ArmorType.Building:
                            return weaker;
                    }
                    break;
            }
            return 1;
        }

        public static ushort GetDamage(CombatObject attacker, CombatObject target, bool useDefAsAtk)
        {
            ushort atk = useDefAsAtk ? attacker.Stats.Def : attacker.Stats.Atk;
            int rawDmg = (atk*attacker.Count)/10;
            double typeModifier = GetArmorTypeModifier(attacker.BaseStats.Weapon, target.BaseStats.Armor);
            double classModifier = GetArmorClassModifier(attacker.BaseStats.WeaponClass, target.BaseStats.ArmorClass);
            rawDmg = (int)(typeModifier*classModifier*rawDmg);
            return rawDmg > ushort.MaxValue ? ushort.MaxValue : (ushort)rawDmg;
        }

        private static int GetLootPerRound(City city) {
            return Config.battle_loot_per_round + city.Technologies.GetEffects(EffectCode.LootLoadMod, EffectInheritance.All).DefaultIfEmpty().Sum(x => x == null ? 0 : (int)x.Value[0]);
        }

        internal static Resource GetRewardResource(CombatObject attacker, CombatObject defender, ushort actualDmg)
        {
            int totalCarry = attacker.BaseStats.Carry*attacker.Count;  // calculate total carry, if 10 units with 10 carry, which should be 100
            int count = Math.Max(1, totalCarry* GetLootPerRound(attacker.City) / 100); // if carry is 100 and % is 5, then count = 5;
            var spaceLeft = new Resource(totalCarry, totalCarry/2, totalCarry/5, totalCarry, 0); // spaceleft is the maxcarry.
            spaceLeft.Subtract(((AttackCombatUnit)attacker).Loot); // maxcarry - current resource is the empty space left.
            return new Resource(Math.Min(count, spaceLeft.Crop),  // returning lesser value between the count and the empty space.
                                Math.Min(count/2, spaceLeft.Gold),
                                Math.Min(count/5, spaceLeft.Iron),
                                Math.Min(count, spaceLeft.Wood),
                                0);
        }

        internal static short GetStamina(TroopStub stub, City city)
        {
            return (short)Config.battle_stamina_initial;
        }

        internal static ushort GetStaminaReinforced(City city, ushort stamina, uint round)
        {
            return stamina;
        }

        internal static ushort GetStaminaRoundEnded(City city, ushort stamina, uint round)
        {
            if (stamina == 0)
                return 0;
            return --stamina;
        }

        internal static short GetStaminaStructureDestroyed(short stamina)
        {
            if (stamina < Config.battle_stamina_destroyed_deduction)
                return 0;

            return (short)(stamina - Config.battle_stamina_destroyed_deduction);
        }

        internal static ushort GetStaminaDefenseCombatObject(City city, ushort stamina, uint round)
        {
            if (stamina == 0)
                return 0;

            return --stamina;
        }

        internal static bool IsAttackMissed(byte stealth)
        {
            return 100 - stealth < Config.Random.Next(0, 100);
        }

        internal static bool UnitStatModCheck(BaseBattleStats stats, TroopBattleGroup group, object comparison, object value)
        {
            switch((string)comparison)
            {
                case "ArmorEqual":
                    return stats.Armor == (ArmorType)Enum.Parse(typeof(ArmorType), (string)value, true);
                case "ArmorClassEqual":
                    return stats.ArmorClass == (ArmorClass)Enum.Parse(typeof(ArmorClass), (string)value, true);
                case "WeaponEqual":
                    return stats.Weapon == (WeaponType)Enum.Parse(typeof(WeaponType), (string)value, true);
                case "WeaponClassEqual":
                    return stats.WeaponClass == (WeaponClass)Enum.Parse(typeof(WeaponClass), (string)value, true);
                case "GroupEqual":
                    return group == (TroopBattleGroup)Enum.Parse(typeof(TroopBattleGroup), (string)value, true);
            }
            return false;
        }

        internal static BattleStats LoadStats(BaseBattleStats stats, City city, TroopBattleGroup group)
        {
            var calculator = new BattleStatsModCalculator(stats);
            foreach (var effect in city.Technologies.GetAllEffects(EffectInheritance.All)) {
                if (effect.Id == EffectCode.UnitStatMod) {
                    if (UnitStatModCheck(stats, group, effect.Value[3], effect.Value[4])) {
                        switch ((string)effect.Value[0]) {
                            case "Atk":
                                calculator.Atk.AddMod((string)effect.Value[1], (int)effect.Value[2]);
                                break;
                            case "Splash":
                                calculator.Splash.AddMod((string)effect.Value[1], (int)effect.Value[2]);
                                break;
                            case "Def":
                                calculator.Def.AddMod((string)effect.Value[1], (int)effect.Value[2]);
                                break;
                            case "Spd":
                                calculator.Spd.AddMod((string)effect.Value[1], (int)effect.Value[2]);
                                break;
                            case "Stl":
                                calculator.Stl.AddMod((string)effect.Value[1], (int)effect.Value[2]);
                                break;
                            case "Rng":
                                calculator.Rng.AddMod((string)effect.Value[1], (int)effect.Value[2]);
                                break;
                            case "Carry":
                                calculator.Carry.AddMod((string)effect.Value[1], (int)effect.Value[2]);
                                break;
                            case "MaxHp":
                                calculator.MaxHp.AddMod((string)effect.Value[1], (int)effect.Value[2]);
                                break;
                        }
                    }
                } else if (effect.Id == EffectCode.ACallToArmMod && group == TroopBattleGroup.Local)
                    calculator.Def.AddMod("PERCENT_BONUS", 100 + (((int)effect.Value[0] * city.Resource.Labor.Value) / (city.MainBuilding.Lvl * 100)));
            }
            return calculator.GetStats();
        }

        internal static BattleStats LoadStats(Structure structure)
        {
            return LoadStats(structure.Stats.Base.Battle,structure.City,TroopBattleGroup.Local);
        }

        internal static BattleStats LoadStats(ushort type, byte lvl, City city, TroopBattleGroup group)
        {
            return LoadStats(UnitFactory.GetUnitStats(type, lvl).Battle,city,group);
        }

        public static Resource GetBonusResources(TroopObject troop)
        {
            int max = troop.City.Technologies.GetEffects(EffectCode.SunDance, EffectInheritance.Self).DefaultIfEmpty().Sum(x => x != null ? (int)x.Value[0] : 0);
            var bonus = new Resource(troop.Stats.Loot) * ((Config.Random.NextDouble() + 1f) * (100 + max) / 100f);
            return bonus;
        }

        public static int GetNumberOfHits(CombatObject currentAttacker)
        {
            return currentAttacker.Stats.Splash == 0 ? 1 : currentAttacker.Stats.Splash;
        }
    }
}