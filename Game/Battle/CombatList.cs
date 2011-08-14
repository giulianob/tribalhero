#region

using System;
using System.Collections.Generic;
using System.Linq;
using Game.Data;
using Game.Data.Troop;
using Game.Database;
using Game.Setup;

#endregion

namespace Game.Battle
{
    public class CombatList : PersistableList<CombatObject>
    {
        #region BestTargetResult enum

        public enum BestTargetResult
        {
            NoneInRange,
            NoneVisible,
            Ok
        }

        #endregion

        public int Id { get; set; }

        public int Upkeep
        {
            get
            {
                return this.Sum(obj => obj.Upkeep);
            }
        }

        public bool HasInRange(CombatObject attacker)
        {
            return this.Any(obj => (obj.InRange(attacker) && attacker.InRange(obj)) && !obj.IsDead);
        }

        public BestTargetResult GetBestTargets(CombatObject attacker, out List<CombatObject> result, int maxCount)
        {
            result = null;
            CombatObject bestTarget = null;
            int bestTargetScore = 0;

            bool hasInRange = false;

            var objectsByScore = new List<CombatScoreItem>(Count);

            foreach (var obj in this)
            {
                if (!obj.InRange(attacker) || !attacker.InRange(obj) || obj.IsDead)
                    continue;

                hasInRange = true;

                if (!attacker.CanSee(obj,this.Min(x=>x.Stats.Stl)))
                    continue;

                int score = 0;

                uint x1, y1, x2, y2;
                attacker.Location(out x1, out y1);
                obj.Location(out x2, out y2);
                score += Math.Max(3 - SimpleGameObject.RadiusDistance(x1, y1, x2, y2) * 2, 0);  // distance 0 gives 60% higher chance to hit, distance 1 gives 20%

                //have to compare armor and weapon type here to give some sort of score
                score += ((int)(BattleFormulas.GetDmgModifier(attacker, obj) * 10));

                score += Config.Random.Next(5); // just add some randomness

                if (bestTarget == null || score > bestTargetScore)
                {
                    bestTarget = obj;
                    bestTargetScore = score;
                }

                objectsByScore.Add(new CombatScoreItem {CombatObject = obj, Score = score});
            }

            if (bestTarget == null)
                return !hasInRange ? BestTargetResult.NoneInRange : BestTargetResult.NoneVisible;

            if (BattleFormulas.IsAttackMissed(bestTarget.Stats.Stl))
            {
                if (objectsByScore.Count == 1)
                    return BestTargetResult.Ok;

                objectsByScore.RemoveAt(0);
            }

            // Sort by score descending
            objectsByScore.Sort((x, y) => x.Score.CompareTo(y.Score)*-1);

            // Get top results specified by the maxCount param
            result = objectsByScore.GetRange(0, Math.Min(maxCount, objectsByScore.Count)).Select(x => x.CombatObject).ToList();
#if DEBUG
            foreach( var obj in objectsByScore)
            {

                Global.Logger.Debug(string.Format("Unit[{0}] Score[{1}]",
                                                        obj.CombatObject.ClassType == BattleClass.Unit ? UnitFactory.GetName(obj.CombatObject.Type, 1) : StructureFactory.GetName(obj.CombatObject.Type, 1),
                                                        obj.Score));
            }
#endif 
            return BestTargetResult.Ok;
        }

        public new void Add(CombatObject item)
        {
            Add(item, true);
        }

        public new void Add(CombatObject item, bool save)
        {
            base.Add(item, save);
            item.CombatList = this;
        }

        public bool Contains(TroopStub obj)
        {
            foreach (var currObj in this)
            {
                if (currObj.CompareTo(obj) == 0)
                    return true;
            }

            return false;
        }

        public bool Contains(Structure obj)
        {
            foreach (var currObj in this)
            {
                if (currObj.CompareTo(obj) == 0)
                    return true;
            }

            return false;
        }

        #region Nested type: CombatScoreItem

        public class CombatScoreItem
        {
            public int Score { get; set; }
            public CombatObject CombatObject { get; set; }
        }

        #endregion

        #region Nested type: NoneInRange

        public class NoneInRange : Exception
        {
        }

        #endregion

        #region Nested type: NoneVisible

        public class NoneVisible : Exception
        {
        }

        #endregion
    }
}