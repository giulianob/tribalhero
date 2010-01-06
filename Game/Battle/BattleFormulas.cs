using System;
using System.Collections.Generic;
using System.Text;
using Game.Data;
using Game.Setup;
using Game.Data.Stats;

namespace Game.Battle {
    public class BattleFormulas {
        public static double getArmorTypeModifier(WeaponType weapon, ArmorType armor) {
            double Weakest = 0.5;
            double Weak = 0.75;
            double Good = 1;
            double Strong = 1.25;
            double Strongest = 1.5;

            switch (weapon) {
                case WeaponType.Sword:
                    switch (armor) {
                        case ArmorType.Leather:
                            return Good;
                        case ArmorType.Metal:
                            return Weak;
                        case ArmorType.Mount:
                            return Weakest;
                        case ArmorType.Wooden:
                            return Good;
                        case ArmorType.Stone:
                            return Strong;
                    }
                    break;
                case WeaponType.Pike:
                    switch (armor) {
                        case ArmorType.Leather:
                            return Weak;
                        case ArmorType.Metal:
                            return Strong;
                        case ArmorType.Mount:
                            return Good;
                        case ArmorType.Wooden:
                            return Weak;
                        case ArmorType.Stone:
                            return Weak;
                    }
                    break;
                case WeaponType.Bow:
                    switch (armor) {
                        case ArmorType.Leather:
                            return Strongest;
                        case ArmorType.Metal:
                            return Weak;
                        case ArmorType.Mount:
                            return Good;
                        case ArmorType.Wooden:
                            return Weak;
                        case ArmorType.Stone:
                            return Weakest;
                    }
                    break;
                case WeaponType.FireBall:
                    switch (armor) {
                        case ArmorType.Leather:
                            return Weak;
                        case ArmorType.Metal:
                            return Good;
                        case ArmorType.Mount:
                            return Strong;
                        case ArmorType.Wooden:
                            return Good;
                        case ArmorType.Stone:
                            return Weak;
                    }
                    break;
                case WeaponType.StoneBall:
                    switch (armor) {
                        case ArmorType.Leather:
                            return Weakest;
                        case ArmorType.Metal:
                            return Weakest;
                        case ArmorType.Mount:
                            return Weak;
                        case ArmorType.Wooden:
                            return Strong;
                        case ArmorType.Stone:
                            return Strongest;
                    }
                    break;
            }
            return 1;
        }

        public static ushort getDamage(CombatObject attacker, CombatObject target, bool useDefAsAtk) {
            if (true) {
                int rawDmg = (useDefAsAtk ? attacker.Stats.Def : attacker.Stats.Atk) * attacker.Count;
                rawDmg /= 10;
                double typeModifier = getArmorTypeModifier(attacker.BaseStats.Weapon, target.BaseStats.Armor);
                rawDmg = (int)(typeModifier * rawDmg);
                return rawDmg > ushort.MaxValue ? ushort.MaxValue : (ushort)rawDmg;
            } 
            
            /*else if (true) {
                int rawDmg = (int)(attacker.Stats.Atk * attacker.Count);
                int drate = target.Stats.Def;
                int arate = attacker.Stats.Atk;
                if (drate > arate / 2) {
                    int deduction = 100 - (drate - arate) * 2 * 100 / ((drate + arate) * 3);
                    rawDmg = rawDmg * deduction / 100;
                } else {

                }
                return rawDmg > ushort.MaxValue ? ushort.MaxValue : (ushort)rawDmg;
            } else {
                double atk_rate = (attacker.Stats.Atk * attacker.Count);
                double def_rate = (target.Stats.Def * target.Count + 1);
                double power_modifier = Math.Pow(atk_rate / def_rate, .1);
                double type_modifier = getArmorTypeModifier(attacker.Stats.Weapon, target.Stats.Armor);
                double base_dmg = attacker.Stats.Atk * attacker.Count;
                double ret = base_dmg * power_modifier * type_modifier;
                return ret > ushort.MaxValue ? ushort.MaxValue : (ushort)ret;
            }*/
        }


        internal static Resource getRewardResource(CombatObject attacker, CombatObject defender, int actualDmg) {
            int point = actualDmg * defender.Stats.Base.Reward;
            switch (defender.ClassType) {
                case BattleClass.Structure:
                    return new Resource(point / 5, point / 7, point / 9, point / 5, 0);
                case BattleClass.Unit:
                    return new Resource(point / 11, point / 15, point / 19, point / 11, 0);
            }

            return new Resource();
        }

        internal static ushort getStamina(City city) {
            return (ushort)(city.MainBuilding.Lvl * 5 + 10 + Config.stamina_initial);
        }

        internal static ushort getStaminaReinforced(City city, ushort stamina, uint round) {
            if (round >= city.MainBuilding.Lvl*5 ) return stamina;
            return (ushort)(stamina + city.MainBuilding.Lvl*5 - round);
        }

        internal static ushort getStaminaRoundEnded(City city, ushort stamina, uint round) {
            return --stamina;
        }

        internal static ushort getStaminaStructureDestroyed(City city, ushort stamina, uint round) {
            if (stamina <= 10) return 0;
            return (ushort)(stamina - 10);
        }

        internal static bool IsAttackMissed(byte stealth) {
            return 100-stealth > Setup.Config.Random.Next(0, 100);
        }

        internal static BattleStats LoadStats(Structure structure) {          
            return new BattleStats(structure.Stats.Base.Battle);
        }

        internal static BattleStats LoadStats(ushort type, byte lvl, City city) {
            int hp = 0;
            int atk = 0;

            BaseBattleStats stats = UnitFactory.getUnitStats(type, lvl).Battle;
            BattleStats modifiedStats = new BattleStats(stats);

            foreach (Effect effect in city.Technologies.GetAllEffects(EffectInheritance.All)) {
                if (effect.id == EffectCode.BattleStatsArmoryMod && stats.Armor == (ArmorType)Enum.Parse(typeof(ArmorType), (string)effect.value[0])) {
                    hp = Math.Max((int)effect.value[1], hp);
                }

                if (effect.id == EffectCode.BattleStatsBlacksmithMod && stats.Weapon == (WeaponType)Enum.Parse(typeof(WeaponType), (string)effect.value[0])) {
                    atk = Math.Max((int)effect.value[1], atk);
                }
            }

            modifiedStats.MaxHp = (ushort)((100 + hp) * stats.MaxHp / 100);
            modifiedStats.Atk = (byte)((100 + atk) * stats.Atk / 100);
            modifiedStats.Def = (byte)((100 + atk) * stats.Def / 100);

            return modifiedStats;
        }
    }
}
