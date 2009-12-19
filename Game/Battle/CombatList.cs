using System;
using System.Collections.Generic;
using System.Text;
using Game.Data;
using Game.Database;

namespace Game.Battle {
    public class CombatList : PersistableList<CombatObject> {

        public enum BestTargetResult {
            NoneInRange,
            NoneVisible,
            Ok
        }

        public class NoneInRange : Exception {
        }

        public class NoneVisible : Exception {
        }

        int id;
        public int Id {
            get { return id; }
            set { id = value; }
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
                score += ((int)BattleFormulas.getArmorTypeModifier(attacker.BaseStats.Weapon, obj.BaseStats.Armor) * 10);

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
                if (!hasInRange)
                    return BestTargetResult.NoneInRange;
                else
                    return BestTargetResult.NoneVisible;
            }
            else {
                if (BattleFormulas.IsAttackMissed(bestTarget.Stats.Stl)) {
                    result = this[Setup.Config.Random.Next(Count)];
                    return BestTargetResult.Ok;
                }
                result = bestTarget;
                return BestTargetResult.Ok;
            }
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
