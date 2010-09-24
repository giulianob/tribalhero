#region

using System;
using Game.Data;
using Game.Data.Stats;
using Game.Setup;
using Game.Data.Troop;

#endregion

namespace Game.Battle
{
    public class BattleFormulas
    {
        public static int MissChance(bool isAttacker, int numberOfDefenders, int numberOfAttackers) {
            int delta = isAttacker ? Math.Max(0, numberOfAttackers - numberOfDefenders) : Math.Max(0, numberOfDefenders - numberOfAttackers);

            return Math.Min(delta * 3, 40);
        }

        public static double GetArmorClassModifier(WeaponClass weapon, ArmorClass armor)
        {
            switch (weapon)
            {
                case WeaponClass.BASIC:
                    switch (armor)
                    {
                        case ArmorClass.LEATHER:
                        case ArmorClass.WOODEN:
                            return 1;
                        case ArmorClass.METAL:
                        case ArmorClass.STONE:
                            return 0.6;
                    }
                    break;
                case WeaponClass.ELEMENTAL:
                    switch (armor)
                    {
                        case ArmorClass.LEATHER:
                        case ArmorClass.WOODEN:
                            return 0.75;
                        case ArmorClass.METAL:
                        case ArmorClass.STONE:
                            return 2;
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
            const double strong = 1.5;
            const double stronger = 2.2;
            const double strongest = 3;

            switch (weapon)
            {
                case WeaponType.SWORD:
                    switch (armor)
                    {
                        case ArmorType.GROUND:
                            return strong;
                        case ArmorType.MOUNT:
                            return weak;
                        case ArmorType.MACHINE:
                            return strong;
                        case ArmorType.BUILDING:
                            return weaker;
                    }
                    break;
                case WeaponType.PIKE:
                    switch (armor)
                    {
                        case ArmorType.GROUND:
                            return weak;
                        case ArmorType.MOUNT:
                            return stronger;
                        case ArmorType.MACHINE:
                            return weak;
                        case ArmorType.BUILDING:
                            return weaker;
                    }
                    break;
                case WeaponType.BOW:
                    switch (armor)
                    {
                        case ArmorType.GROUND:
                            return strong;
                        case ArmorType.MOUNT:
                            return good;
                        case ArmorType.MACHINE:
                            return weakest;
                        case ArmorType.BUILDING:
                            return nodamage;
                    }
                    break;
                case WeaponType.BALL:
                    switch (armor)
                    {
                        case ArmorType.GROUND:
                            return nodamage;
                        case ArmorType.MOUNT:
                            return nodamage;
                        case ArmorType.MACHINE:
                            return good;
                        case ArmorType.BUILDING:
                            return strongest;
                    }
                    break;
                case WeaponType.BARRICADE:
                    switch (armor)
                    {
                        case ArmorType.GROUND:
                            return weaker;
                        case ArmorType.MOUNT:
                            return weaker;
                        case ArmorType.MACHINE:
                            return weaker;
                        case ArmorType.BUILDING:
                            return weaker;
                    }
                    break;
            }
            return 1;
        }

        public static ushort GetDamage(CombatObject attacker, CombatObject target, bool useDefAsAtk)
        {
            ushort atk = useDefAsAtk ? attacker.Stats.Def : attacker.Stats.Atk;
            int rawDmg = atk * attacker.Count;
            rawDmg /= 10;
            double typeModifier = GetArmorTypeModifier(attacker.BaseStats.Weapon, target.BaseStats.Armor);
            double classModifier = GetArmorClassModifier(attacker.BaseStats.WeaponClass, target.BaseStats.ArmorClass);
            rawDmg = (int)(typeModifier * classModifier * rawDmg);
            return rawDmg > ushort.MaxValue ? ushort.MaxValue : (ushort)rawDmg;
        }

        internal static Resource GetRewardResource(CombatObject attacker, CombatObject defender, ushort actualDmg)
        {
            int totalCarry = attacker.BaseStats.Carry * attacker.Count;
            int count = Math.Max(1, attacker.BaseStats.Carry * attacker.Count * Config.battle_loot_per_round / 100);
            Resource spaceLeft = new Resource(totalCarry, totalCarry, totalCarry, totalCarry, 0);
            spaceLeft.Subtract(((AttackCombatUnit)attacker).Loot);
            return new Resource(Math.Min(count, spaceLeft.Crop), Math.Min(count, spaceLeft.Gold / 2), Math.Min(count, spaceLeft.Iron), Math.Min(count, spaceLeft.Wood), 0);
        }

        internal static ushort GetStamina(City city)
        {
            return (ushort)(Config.battle_stamina_initial);
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

        internal static ushort GetStaminaStructureDestroyed(City city, ushort stamina, uint round)
        {
            if (stamina < Config.battle_stamina_destroyed_deduction)
                return 0;
            return (ushort)(stamina - Config.battle_stamina_destroyed_deduction);
        }

        internal static bool IsAttackMissed(byte stealth)
        {
            return 100 - stealth < Config.Random.Next(0, 100);
        }

        internal static bool UnitStatModCheck(BaseBattleStats stats, object comparison, object value)
        {
            switch ((string)comparison)
            {
                case "ArmorEqual":
                    return stats.Armor == (ArmorType)Enum.Parse(typeof(ArmorType), (string)value, true);
                case "ArmorClassEqual":
                    return stats.ArmorClass == (ArmorClass)Enum.Parse(typeof(ArmorClass), (string)value, true);
                case "WeaponEqual":
                    return stats.Weapon == (WeaponType)Enum.Parse(typeof(WeaponType), (string)value, true);
                case "WeaponClassEqual":
                    return stats.WeaponClass == (WeaponClass)Enum.Parse(typeof(WeaponClass), (string)value, true);
            }
            return false;
        }

        internal static BattleStats LoadStats(Structure structure)
        {
            return new BattleStats(structure.Stats.Base.Battle);
        }

        internal static BattleStats LoadStats(ushort type, byte lvl, City city, TroopBattleGroup group)
        {
            BaseBattleStats stats = UnitFactory.GetUnitStats(type, lvl).Battle;
            BattleStatsModCalculator calculator = new BattleStatsModCalculator(stats);

            foreach (Effect effect in city.Technologies.GetAllEffects(EffectInheritance.ALL))
            {
                if (effect.id == EffectCode.UnitStatMod)
                {
                    if (UnitStatModCheck(stats, effect.value[3], effect.value[4]))
                    {
                        switch ((string)effect.value[0])
                        {
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
                }
                else if (effect.id == EffectCode.ACallToArmMod && group == TroopBattleGroup.LOCAL)
                    calculator.Def.AddMod("PERCENT_BONUS", 100 + (((int)effect.value[0] * city.Resource.Labor.Value) / (city.MainBuilding.Lvl * 100)));
            }
            return calculator.GetStats();
        }

        public static Resource GetBonusResources(TroopObject troop)
        {
            Resource bonus = new Resource(troop.Stats.Loot);
            bonus *= (Config.Random.NextDouble() + 1.0);
            return bonus;
        }
    }
}