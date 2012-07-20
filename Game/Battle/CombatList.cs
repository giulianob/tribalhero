#region

using System;
using System.Collections.Generic;
using System.Linq;
using Game.Battle.CombatGroups;
using Game.Battle.CombatObjects;
using Game.Map;
using Persistance;

#endregion

namespace Game.Battle
{
    /// <summary>
    /// A list of combat objects and manages targetting.
    /// </summary>
    public class CombatList : ListOfPersistableObjects<CombatGroup>, ICombatList
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

        public CombatList(IDbManager manager, RadiusLocator radiusLocator, BattleFormulas battleFormulas)
                : base(manager)
        {
            this.radiusLocator = radiusLocator;
            this.battleFormulas = battleFormulas;
        }

        public int Upkeep
        {
            get
            {
                return AllCombatObjects().Sum(obj => obj.Upkeep);
            }
        }

        public bool HasInRange(CombatObject attacker)
        {
            return AllCombatObjects().Any(obj => (obj.InRange(attacker) && attacker.InRange(obj)) && !obj.IsDead);
        }

        public BestTargetResult GetBestTargets(CombatObject attacker, out IList<Target> result, int maxCount)
        {
            result = new List<Target>();

            var objectsByScore = new List<CombatScoreItem>(Count);

            var objsInRange = (from @group in this
                               from combatObject in @group
                               where combatObject.InRange(attacker) && attacker.InRange(combatObject) && !combatObject.IsDead
                               select new Target {Group = @group, CombatObject = combatObject}).ToList();

            if (objsInRange.Count == 0)
            {
                return BestTargetResult.NoneInRange;
            }

            uint lowestRow = objsInRange.Min(target => target.CombatObject.Stats.Stl);

            uint x1, y1;
            attacker.Location(out x1, out y1);

            Target bestTarget = null;
            int bestTargetScore = 0;
            foreach (var target in objsInRange)
            {
                if (!attacker.CanSee(target.CombatObject, lowestRow))
                {
                    continue;
                }

                int score = 0;

                uint x2, y2;
                target.CombatObject.Location(out x2, out y2);

                // Distance 0 gives 60% higher chance to hit, distance 1 gives 20%
                score += Math.Max(3 - radiusLocator.RadiusDistance(x1, y1, x2, y2)*2, 0);

                // Have to compare armor and weapon type here to give some sort of score
                score += ((int)(battleFormulas.GetDmgModifier(attacker, target.CombatObject)*10));

                if (bestTarget == null || score > bestTargetScore)
                {
                    bestTarget = target;
                    bestTargetScore = score;
                }

                objectsByScore.Add(new CombatScoreItem {Target = target, Score = score});
            }

            // Sort by score descending
            objectsByScore.Sort((x, y) => x.Score.CompareTo(y.Score)*-1);

            // Get top results specified by the maxCount param
            result = objectsByScore.GetRange(0, Math.Min(maxCount, objectsByScore.Count)).Select(scoreItem => scoreItem.Target).ToList();

            return BestTargetResult.Ok;
        }

        public IEnumerable<CombatObject> AllCombatObjects()
        {
            return BackingList.SelectMany(group => group.Select(combatObject => combatObject));
        }

        #region Nested type: CombatScoreItem

        private class CombatScoreItem
        {
            public int Score { get; set; }

            public Target Target { get; set; }
        }

        #endregion

        #region Nested type: Target

        public class Target
        {
            public CombatObject CombatObject { get; set; }

            public CombatGroup Group { get; set; }
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