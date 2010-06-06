#region

using System;
using Game.Data;
using Game.Data.Stats;
using Game.Setup;
using Game.Logic.Conditons;
using Game.Data.Troop;

#endregion

namespace Game.Battle {
    public class BattleFormulas {
        public static double GetArmorTypeModifier(WeaponType weapon, ArmorType armor) {
            const double weakest = 0.2;
            const double weak = 0.6;
            const double good = 1;
            const double strong = 1.4;
            const double strongest = 1.8;

            switch (weapon) {
                case WeaponType.SWORD:
                    switch (armor) {
                        case ArmorType.LEATHER:
                            return good;
                        case ArmorType.METAL:
                            return weak;
                        case ArmorType.MOUNT:
                            return weakest;
                        case ArmorType.WOODEN:
                            return good;
                        case ArmorType.STONE:
                            return weak;
                    }
                    break;
                case WeaponType.PIKE:
                    switch (armor) {
                        case ArmorType.LEATHER:
                            return weak;
                        case ArmorType.METAL:
                            return strong;
                        case ArmorType.MOUNT:
                            return good;
                        case ArmorType.WOODEN:
                            return weak;
                        case ArmorType.STONE:
                            return weak;
                    }
                    break;
                case WeaponType.BOW:
                    switch (armor) {
                        case ArmorType.LEATHER:
                            return strongest;
                        case ArmorType.METAL:
                            return weak;
                        case ArmorType.MOUNT:
                            return good;
                        case ArmorType.WOODEN:
                            return weak;
                        case ArmorType.STONE:
                            return weakest;
                    }
                    break;
                case WeaponType.FIRE_BALL:
                    switch (armor) {
                        case ArmorType.LEATHER:
                            return weak;
                        case ArmorType.METAL:
                            return good;
                        case ArmorType.MOUNT:
                            return strong;
                        case ArmorType.WOODEN:
                            return good;
                        case ArmorType.STONE:
                            return weak;
                    }
                    break;
                case WeaponType.STONE_BALL:
                    switch (armor) {
                        case ArmorType.LEATHER:
                            return weakest;
                        case ArmorType.METAL:
                            return weakest;
                        case ArmorType.MOUNT:
                            return weak;
                        case ArmorType.WOODEN:
                            return strong;
                        case ArmorType.STONE:
                            return strongest;
                    }
                    break;
            }
            return 1;
        }

        public static ushort GetDamage(CombatObject attacker, CombatObject target, bool useDefAsAtk) {
            ushort atk = useDefAsAtk ? attacker.Stats.Def : attacker.Stats.Atk;
            int rawDmg = atk * attacker.Count;
            rawDmg /= 10;
            double typeModifier = GetArmorTypeModifier(attacker.BaseStats.Weapon, target.BaseStats.Armor);
            rawDmg = (int) (typeModifier*rawDmg);
            return rawDmg > ushort.MaxValue ? ushort.MaxValue : (ushort) rawDmg;           
        }

        internal static Resource GetRewardResource(CombatObject attacker, CombatObject defender, ushort actualDmg) {
            int totalCarry = attacker.BaseStats.Carry * attacker.Count;
            int count = attacker.BaseStats.Carry * attacker.Count * Config.battle_loot_per_round / 100;
            if (count == 0) count = 1;
            Resource empty = new Resource(totalCarry, totalCarry, totalCarry, totalCarry, 0);
            empty.subtract(((AttackCombatUnit) attacker).Loot);
            return new Resource(Math.Min(count, empty.Crop),
                                         Math.Min(count, empty.Gold),
                                         Math.Min(count, empty.Iron),
                                         Math.Min(count, empty.Wood),
                                         0);
        }

        internal static ushort GetStamina(City city) {
            return (ushort)(20 + Config.battle_stamina_initial);
        }

        internal static ushort GetStaminaReinforced(City city, ushort stamina, uint round) {
            if (round >= city.MainBuilding.Lvl*5)
                return stamina;
            return (ushort) (stamina + city.MainBuilding.Lvl*5 - round);
        }

        internal static ushort GetStaminaRoundEnded(City city, ushort stamina, uint round) {
            return --stamina;
        }

        internal static ushort GetStaminaStructureDestroyed(City city, ushort stamina, uint round) {
            if (stamina <= 10)
                return 0;
            return (ushort) (stamina - 10);
        }

        internal static bool IsAttackMissed(byte stealth) {
            return 100 - stealth > Config.Random.Next(0, 100);
        }

        internal static bool UnitStatModCheck(BaseBattleStats stats, object comparison, object value) {
            switch ((string)comparison) {
                case "ArmorEqual":
                    return stats.Armor == (ArmorType)Enum.Parse(typeof(ArmorType), (string)value, true);
                case "WeaponEqual":
                    return stats.Weapon == (WeaponType)Enum.Parse(typeof(WeaponType), (string)value, true);
            }
            return false;
        }

        internal static BattleStats LoadStats(Structure structure) {
            return new BattleStats(structure.Stats.Base.Battle);
        }

        internal static BattleStats LoadStats(ushort type, byte lvl, City city, TroopBattleGroup group) {
            BaseBattleStats stats = UnitFactory.GetUnitStats(type, lvl).Battle;
            BattleStatsModCalculator calculator = new BattleStatsModCalculator(stats);

            foreach (Effect effect in city.Technologies.GetAllEffects(EffectInheritance.ALL)) {
                if (effect.id == EffectCode.UnitStatMod) {
                    if (UnitStatModCheck(stats,effect.value[3],effect.value[4])) {
                        switch ((string)effect.value[0]) {
                            case "Atk":
                                calculator.Atk.AddMod((string)effect.value[1], (int)effect.value[2]);
                                break;
                            case "Def":
                                calculator.Def.AddMod((string)effect.value[1], (int)effect.value[2]);
                                break;
                            case "Spd":
                                calculator.Spd.AddMod((string)effect.value[1], (int)effect.value[2]);
                                break;
                            case "Stl":
                                calculator.Stl.AddMod((string)effect.value[1], (int)effect.value[2]);
                                break;
                            case "Rng":
                                calculator.Rng.AddMod((string)effect.value[1], (int)effect.value[2]);
                                break;
                            case "MaxHp":
                                calculator.MaxHp.AddMod((string)effect.value[1], (int)effect.value[2]);
                                break;
                        }
                    }
                } else if(effect.id == EffectCode.ACallToArmMod && group== TroopBattleGroup.LOCAL) {
                    calculator.Def.AddMod("PERCENT_BONUS", 100 + (((int)effect.value[0] * city.Resource.Labor.Value) / (city.MainBuilding.Lvl * 100)));
                }
            }
            return calculator.GetStats();
        }
    }
}