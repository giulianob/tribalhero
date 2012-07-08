#region

using System;
using System.Collections.Generic;
using System.Linq;
using Game.Data;
using Game.Data.Troop;
using Game.Map;
using Persistance;

#endregion

namespace Game.Battle
{
    public interface ICombatList : IListOfPersistableObjects<CombatObject>
    {
        int Upkeep { get; }

        int Capacity { get; set; }

        bool HasInRange(CombatObject attacker);

        CombatList.BestTargetResult GetBestTargets(CombatObject attacker, out IList<CombatObject> result, int maxCount);
    }

    public class CombatList : ListOfPersistableObjects<CombatObject>, ICombatList
    {
        private readonly RadiusLocator radiusLocator;
        private readonly BattleFormulas battleFormulas;

        #region BestTargetResult enum

        public enum BestTargetResult
        {
            NoneInRange,
            Ok
        }

        #endregion

        public CombatList(IDbManager manager, RadiusLocator radiusLocator, BattleFormulas battleFormulas) : base(manager)
        {
            this.radiusLocator = radiusLocator;
            this.battleFormulas = battleFormulas;
        }

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

        public BestTargetResult GetBestTargets(CombatObject attacker, out IList<CombatObject> result, int maxCount)
        {
            result = new List<CombatObject>();
            
            CombatObject bestTarget = null;

            int bestTargetScore = 0;

            var objectsByScore = new List<CombatScoreItem>(Count);

            var objsInRange = this.Where(obj => obj.InRange(attacker) && attacker.InRange(obj) && !obj.IsDead).ToList();

            if (objsInRange.Count == 0)
            {
                return BestTargetResult.NoneInRange;                
            }

            uint lowestRow = objsInRange.Min(obj => obj.Stats.Stl);

            foreach (var obj in objsInRange)
            {
                if (!attacker.CanSee(obj, lowestRow))
                    continue;

                int score = 0;

                uint x1, y1, x2, y2;
                attacker.Location(out x1, out y1);
                obj.Location(out x2, out y2);

                // Distance 0 gives 60% higher chance to hit, distance 1 gives 20%
                score += Math.Max(3 - radiusLocator.RadiusDistance(x1, y1, x2, y2) * 2, 0);

                // Have to compare armor and weapon type here to give some sort of score
                score += ((int)(battleFormulas.GetDmgModifier(attacker, obj) * 10));

                if (bestTarget == null || score > bestTargetScore)
                {
                    bestTarget = obj;
                    bestTargetScore = score;
                }

                objectsByScore.Add(new CombatScoreItem {CombatObject = obj, Score = score});
            }

            // Sort by score descending
            objectsByScore.Sort((x, y) => x.Score.CompareTo(y.Score)*-1);

            // Get top results specified by the maxCount param
            result = objectsByScore.GetRange(0, Math.Min(maxCount, objectsByScore.Count)).Select(x => x.CombatObject).ToList();

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

        #region Nested type: CombatScoreItem

        class CombatScoreItem
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