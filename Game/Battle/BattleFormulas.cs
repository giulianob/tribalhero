#region

using System;
using System.Linq;
using Game.Battle.CombatObjects;
using Game.Data;
using Game.Data.BarbarianTribe;
using Game.Data.Stats;
using Game.Data.Stronghold;
using Game.Data.Troop;
using Game.Setup;
using Game.Util;
using Ninject.Extensions.Logging;

#endregion

namespace Game.Battle
{
    public class BattleFormulas : IBattleFormulas
    {
        [Obsolete("Inject BattleFormulas instead")]
        public static IBattleFormulas Current { get; set; }

        private readonly UnitFactory unitFactory;

        private readonly UnitModFactory unitModFactory;

        public BattleFormulas(UnitModFactory unitModFactory, UnitFactory unitFactory)
        {
            this.unitModFactory = unitModFactory;
            this.unitFactory = unitFactory;
        }        

        public virtual decimal GetDmgWithMissChance(int attackersUpkeep, int defendersUpkeep, decimal dmg, IBattleRandom random)
        {
            double delta = Math.Max(0, (double)attackersUpkeep / defendersUpkeep);
            double effectiveness = attackersUpkeep > 200 ? 1 : (double)attackersUpkeep / 200;
            
            int missChance;
            if (delta < 1)
            {
                missChance = (int)(0 * effectiveness);
            }
            else if (delta < 1.25)
            {
                missChance = (int)(10 * effectiveness);
            }
            else if (delta < 1.5)
            {
                missChance = (int)(17 * effectiveness);
            }
            else if (delta < 2)
            {
                missChance = (int)(22 * effectiveness);
            }
            else if (delta < 3.5)
            {
                missChance = (int)(30 * effectiveness);
            }
            else if (delta < 5)
            {
                missChance = (int)(40 * effectiveness);
            }
            else if (delta < 7)
            {
                missChance = (int)(48 * effectiveness);
            }
            else if (delta < 10)
            {
                missChance = (int)(55 * effectiveness);
            }
            else
            {
                missChance = (int)(60 * effectiveness);
            }

            var rand = (int)(random.NextDouble() * 100);

            if (missChance <= 0 || rand > missChance)
            {
                return dmg;
            }
            
            return dmg / 2m;
        }

        public virtual int GetUnitsPerStructure(byte level)
        {
            int[] units = new[] {20, 20, 23, 28, 34, 39, 45, 52, 59, 67, 76, 85, 95, 106, 117, 130};

            return units[Math.Min(level, units.Length - 1)];
        }

        public virtual decimal GetAttackerDmgToDefender(ICombatObject attacker, ICombatObject target, uint round)
        {
            decimal atk = attacker.Stats.Atk;
            decimal rawDmg = (atk * attacker.Count);
            decimal modifier = (decimal)GetDmgModifier(attacker, target, round);
            rawDmg = modifier * rawDmg;

            return rawDmg > ushort.MaxValue ? ushort.MaxValue : rawDmg;
        }

        public virtual double GetDmgModifier(ICombatObject attacker, ICombatObject target, uint round)
        {
            switch(attacker.Stats.Base.Weapon)
            {
                case WeaponType.Tower:
                    switch(target.Stats.Base.Armor)
                    {
                        case ArmorType.Building1:
                            return round < 5 ? unitModFactory.GetModifier(1, 1) * .2 : unitModFactory.GetModifier(1, 1);
                        case ArmorType.Building2:
                            return round < 5 ? unitModFactory.GetModifier(1, 2) * .2 : unitModFactory.GetModifier(1, 2);;
                        case ArmorType.Building3:
                            return unitModFactory.GetModifier(1, 3);
                        case ArmorType.Gate:
                            return unitModFactory.GetModifier(1, 4);
                        default:
                            return unitModFactory.GetModifier(1, target.Type);
                    }
                case WeaponType.Cannon:
                    switch(target.Stats.Base.Armor)
                    {
                        case ArmorType.Building1:
                            return round < 5 ? unitModFactory.GetModifier(2, 1) * .2 : unitModFactory.GetModifier(2, 1);
                        case ArmorType.Building2:
                            return round < 5 ? unitModFactory.GetModifier(2, 2) * .2 : unitModFactory.GetModifier(2, 2); ;
                        case ArmorType.Building3:
                            return unitModFactory.GetModifier(2, 3);
                        case ArmorType.Gate:
                            return unitModFactory.GetModifier(2, 4);
                        default:
                            return unitModFactory.GetModifier(2, target.Type);
                    }
                case WeaponType.Barricade:
                    switch(target.Stats.Base.Armor)
                    {
                        case ArmorType.Building1:
                            return round < 5 ? unitModFactory.GetModifier(3, 1) * .2 : unitModFactory.GetModifier(3, 1);
                        case ArmorType.Building2:
                            return round < 5 ? unitModFactory.GetModifier(3, 2) * .2 : unitModFactory.GetModifier(3, 2); ;
                        case ArmorType.Building3:
                            return unitModFactory.GetModifier(3, 3);
                        case ArmorType.Gate:
                            return unitModFactory.GetModifier(3, 4);
                        default:
                            return unitModFactory.GetModifier(3, target.Type);
                    }
                default:
                    switch(target.Stats.Base.Armor)
                    {
                        case ArmorType.Building1:
                            return round < 5 ? unitModFactory.GetModifier(attacker.Type, 1) * .2 : unitModFactory.GetModifier(attacker.Type, 1);
                        case ArmorType.Building2:
                            return round < 5 ? unitModFactory.GetModifier(attacker.Type, 2) * .2 : unitModFactory.GetModifier(attacker.Type, 2); ;
                        case ArmorType.Building3:
                            return unitModFactory.GetModifier(attacker.Type, 3);
                        case ArmorType.Gate:
                            return unitModFactory.GetModifier(attacker.Type, 4);
                        default:
                            return unitModFactory.GetModifier(attacker.Type, target.Type);
                    }
            }
        }

        public virtual int GetLootPerRoundForCity(ICity city)
        {
            double roundsRequired = Math.Max(5,
                                             Config.battle_loot_till_full -
                                             city.Technologies.GetEffects(EffectCode.LootLoadMod)
                                                 .DefaultIfEmpty()
                                                 .Sum(x => x == null ? 0 : (int)x.Value[0]));
            return (int)Math.Ceiling(100 / roundsRequired);
        }

        public virtual Resource GetRewardResource(ICombatObject attacker, ICombatObject defender)
        {
            // calculate total carry, if 10 units with 10 carry, which should be 100
            int totalCarry = attacker.Stats.Carry * attacker.Count;

            // if carry is 100 and % is 5, then count = 5;
            int lootPerRound = attacker.LootPerRound();
            int count = Math.Max(1, totalCarry * lootPerRound / 100);

            // spaceleft is the maxcarry.
            var spaceLeft = new Resource(totalCarry / 1,
                                         totalCarry / 2,
                                         totalCarry / Config.battle_loot_resource_iron_ratio,
                                         totalCarry / 1,
                                         totalCarry / Config.battle_loot_resource_labor_ratio);

            // maxcarry minus current resource is the empty space left.
            spaceLeft.Subtract(attacker.Loot);

            // returning lesser value between the count and the empty space.
            return new Resource(Math.Min(count / 1, spaceLeft.Crop),
                                Math.Min(count / 2, spaceLeft.Gold),
                                Math.Min(count / Config.battle_loot_resource_iron_ratio, spaceLeft.Iron),
                                Math.Min(count / 1, spaceLeft.Wood));
        }

        public virtual short GetStamina(ITroopStub stub, ICity city)
        {
            return (short)Config.battle_stamina_initial;
        }

        public virtual short GetStamina(ITroopStub stub, IStronghold targetStronghold)
        {
            return (short)(Config.battle_stamina_initial * Config.battle_stamina_gate_multiplier);
        }

        public virtual short GetStamina(ITroopStub stub, IBarbarianTribe barbarianTribe)
        {
            return (short)Config.battle_stamina_initial;
        }

        public virtual ushort GetStaminaReinforced(ICity city, ushort stamina, uint round)
        {
            return stamina;
        }

        public virtual ushort GetStaminaRoundEnded(ICity city, ushort stamina, uint round)
        {
            if (stamina == 0)
            {
                return 0;
            }
            return --stamina;
        }

        public virtual short GetStaminaStructureDestroyed(short stamina, ICombatObject combatStructure)
        {
            if (combatStructure.Stats.Base.Armor != ArmorType.Building3)
            {
                return stamina;
            }

            return Math.Max((short)0, (short)(stamina - Config.battle_stamina_destroyed_deduction));
        }

        public virtual ushort GetStaminaDefenseCombatObject(ICity city, ushort stamina, uint round)
        {
            if (stamina == 0)
            {
                return 0;
            }

            return --stamina;
        }

        public virtual bool UnitStatModCheck(IBaseBattleStats stats, TroopBattleGroup group, string value)
        {
            string[] conditions = value.Split('=', '|');
            int success = 0;
            for (int i = 0; i < conditions.Length / 2; ++i)
            {
                switch(conditions[i * 2])
                {
                    case "ArmorEqual":
                        if (stats.Armor == (ArmorType)Enum.Parse(typeof(ArmorType), conditions[i * 2 + 1], true))
                        {
                            ++success;
                        }
                        break;
                    case "ArmorClassEqual":
                        if (stats.ArmorClass == (ArmorClass)Enum.Parse(typeof(ArmorClass), conditions[i * 2 + 1], true))
                        {
                            ++success;
                        }
                        break;
                    case "WeaponEqual":
                        if (stats.Weapon == (WeaponType)Enum.Parse(typeof(WeaponType), conditions[i * 2 + 1], true))
                        {
                            ++success;
                        }
                        break;
                    case "WeaponClassEqual":
                        if (stats.WeaponClass ==
                            (WeaponClass)Enum.Parse(typeof(WeaponClass), conditions[i * 2 + 1], true))
                        {
                            ++success;
                        }
                        break;
                    case "GroupEqual":
                        switch((TroopBattleGroup)Enum.Parse(typeof(TroopBattleGroup), conditions[i * 2 + 1], true))
                        {
                            case TroopBattleGroup.Defense:
                                if (group == TroopBattleGroup.Local || group == TroopBattleGroup.Defense)
                                {
                                    ++success;
                                }
                                break;
                            case TroopBattleGroup.Attack:
                                if (group == TroopBattleGroup.Attack)
                                {
                                    ++success;
                                }
                                break;
                            case TroopBattleGroup.Local:
                                if (group == TroopBattleGroup.Local)
                                {
                                    ++success;
                                }
                                break;
                        }
                        break;
                    case "TypeEqual":
                        if (stats.Type == ushort.Parse(conditions[i * 2 + 1]))
                        {
                            ++success;
                        }
                        break;
                }
            }
            return success == conditions.Length / 2;
        }

        public virtual BattleStats LoadStats(IBaseBattleStats stats, ICity city, TroopBattleGroup group)
        {
            var calculator = new BattleStatsModCalculator(stats);
            foreach (var effect in city.Technologies.GetAllEffects())
            {
                if (effect.Id == EffectCode.UnitStatMod)
                {
                    if (!UnitStatModCheck(stats, @group, (string)effect.Value[3]))
                    {
                        continue;
                    }

                    switch((string)effect.Value[0])
                    {
                        case "Atk":
                            calculator.Atk.AddMod((string)effect.Value[1], (int)effect.Value[2]);
                            break;
                        case "Splash":
                            calculator.Splash.AddMod((string)effect.Value[1], (int)effect.Value[2]);
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
                else if (effect.Id == EffectCode.ACallToArmMod && group == TroopBattleGroup.Local)
                {
                    var bonus = (int)effect.Value[0] * Math.Min(city.Resource.Labor.Value, (int)effect.Value[1]) /
                                ((int)effect.Value[1]);
                    calculator.Atk.AddMod("CALL_TO_ARM_BONUS", 100 + bonus);
                }
            }

            return calculator.GetStats();
        }

        public virtual BattleStats LoadStats(IStructure structure)
        {
            return LoadStats(structure.Stats.Base.Battle, structure.City, TroopBattleGroup.Local);
        }

        public virtual BattleStats LoadStats(ushort type, byte lvl, ICity city, TroopBattleGroup group)
        {
            return LoadStats(unitFactory.GetUnitStats(type, lvl).Battle, city, group);
        }

        public virtual Resource GetBonusResources(ITroopObject troop, int originalCount, int remainingCount)
        {
            if (originalCount == 0)
            {
                return new Resource();
            }

            int max =
                    troop.City.Technologies.GetEffects(EffectCode.SunDance, EffectInheritance.Self)
                         .DefaultIfEmpty()
                         .Sum(x => x != null ? (int)x.Value[0] : 0);
            float troopsLostPercentage = 1 - remainingCount / (float)originalCount;
            return new Resource(troop.Stats.Loot) * (troopsLostPercentage) * (1f + (Config.Random.Next(max) / 100f));
        }

        public virtual int GetNumberOfHits(ICombatObject currentAttacker)
        {
            return currentAttacker.Stats.Splash == 0 ? 1 : currentAttacker.Stats.Splash;
        }

        public virtual decimal SplashReduction(CityCombatObject defender, decimal dmg, int attackIndex)
        {
            // Splash damage reduction doesnt apply to the first attack
            if (attackIndex == 0)
            {
                return dmg;
            }

            var splashEffects = defender.City.Technologies.GetEffects(EffectCode.SplashReduction,
                                                                      EffectInheritance.SelfAll);
            decimal reduction =
                    splashEffects.Where(
                                        effect =>
                                        UnitStatModCheck(defender.Stats.Base,
                                                         TroopBattleGroup.Defense,
                                                         (string)effect.Value[1]))
                                 .DefaultIfEmpty()
                                 .Max(x => x == null ? 0 : (int)x.Value[0]);

            reduction = (100 - reduction) / 100;
            return reduction * dmg;
        }
    }
}