#region

using System;
using System.Linq;
using Game.Data;
using Game.Data.Troop;
using Game.Database;
using Game.Setup;

#endregion

namespace Game.Battle {
    public class CombatList : PersistableList<CombatObject> {
        public enum BestTargetResult {
            NONE_IN_RANGE,
            NONE_VISIBLE,
            OK
        }

        public class NoneInRange : Exception {}

        public class NoneVisible : Exception {}

        public int Id { get; set; }

        public bool HasInRange(CombatObject attacker) {
            return this.Any(obj => (obj.InRange(attacker) && attacker.InRange(obj)) && !obj.IsDead);
        }

        public BestTargetResult GetBestTarget(CombatObject attacker, out CombatObject result) {
            result = null;
            CombatObject bestTarget = null;
            int bestTargetScore = 0;

            bool hasInRange = false;

            foreach (CombatObject obj in this) {
                if (!obj.InRange(attacker) || !attacker.InRange(obj) || obj.IsDead)
                    continue;

                hasInRange = true;

                if (!attacker.CanSee(obj))
                    continue;

                int score = 0;

                //have to compare armor and weapon type here to give some sort of score
                score += ((int)BattleFormulas.GetArmorTypeModifier(attacker.BaseStats.Weapon, obj.BaseStats.Armor) * 10);
                score += ((int)BattleFormulas.GetArmorClassModifier(attacker.BaseStats.WeaponClass, obj.BaseStats.ArmorClass) * 20);
                /*     if (obj.Stats.Armor == ArmorType.HEAVY && attacker.Stats.Weapon == WeaponType.HEAVY)
                    score += 10;
                else if (obj.Stats.ArmorType == Stats.Armor.LIGHT && attacker.Stats.WeaponType == Stats.Weapon.LIGHT)
                    score += 10;
                if (obj.Stats.ArmorType == Stats.Armor.STRUCTURE && attacker.Stats.WeaponType == Stats.Weapon.STRUCTURE)
                    score += 10;*/

                if (obj.Stats.Def < obj.Stats.Atk)
                    score += 5;

                if (bestTarget == null || score > bestTargetScore) {
                    bestTarget = obj;
                    bestTargetScore = score;
                }
            }

            if (bestTarget == null) {
                return !hasInRange ? BestTargetResult.NONE_IN_RANGE : BestTargetResult.NONE_VISIBLE;
            }

            if (BattleFormulas.IsAttackMissed(bestTarget.Stats.Stl)) {
                result = this[Config.Random.Next(Count)];
                return BestTargetResult.OK;
            }
            
            result = bestTarget;
            return BestTargetResult.OK;
        }

        public new void Add(CombatObject item) {
            Add(item, true);
        }

        public new void Add(CombatObject item, bool save) {
            base.Add(item, save);
            item.CombatList = this;
        }

        public bool Contains(TroopStub obj) {
            foreach (CombatObject currObj in this) {
                if (currObj.CompareTo(obj) == 0)
                    return true;
            }

            return false;
        }

        public bool Contains(Structure obj) {
            foreach (CombatObject currObj in this) {
                if (currObj.CompareTo(obj) == 0)
                    return true;
            }

            return false;
        }
    }
}