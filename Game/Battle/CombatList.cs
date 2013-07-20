#region

using System;
using System.Collections.Generic;
using System.Linq;
using Game.Battle.CombatGroups;
using Game.Battle.CombatObjects;
using Game.Data;
using Game.Map;
using Game.Util;
using Persistance;

#endregion

namespace Game.Battle
{
    /// <summary>
    ///     A list of combat objects and manages targetting.
    /// </summary>
    public class CombatList : PersistableObjectList<ICombatGroup>, ICombatList
    {
        private readonly BattleFormulas battleFormulas;

        private readonly RadiusLocator radiusLocator;

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

        public int UpkeepNotParticipated(uint round)
        {
            return AllAliveCombatObjects().Where(obj => obj.LastRound <= round).Sum(x => x.Upkeep);
        }

        public bool HasInRange(ICombatObject attacker)
        {
            return AllAliveCombatObjects().Any(obj => obj.InRange(attacker) && attacker.InRange(obj));
        }

        public BestTargetResult GetBestTargets(uint battleId, ICombatObject attacker, out List<Target> result, int maxCount)
        {
            result = new List<Target>();

            var objectsByScore = new List<CombatScoreItem>(Count);

            var objsInRange = (from @group in this
                               from combatObject in @group
                               where
                                       combatObject.InRange(attacker) && attacker.InRange(combatObject) &&
                                       !combatObject.IsDead
                               select new Target {Group = @group, CombatObject = combatObject}).ToList();

            if (objsInRange.Count == 0)
            {
                return BestTargetResult.NoneInRange;
            }

            uint lowestRow = objsInRange.Min(target => target.CombatObject.Stats.Stl);

            Target bestTarget = null;
            int bestTargetScore = 0;
            foreach (var target in objsInRange)
            {
                if (!attacker.CanSee(target.CombatObject, lowestRow))
                {
                    continue;
                }

                int score = 0;

                // Have to compare armor and weapon type here to give some sort of score
                score += ((int)(battleFormulas.GetDmgModifier(attacker, target.CombatObject) * 10));

                if (bestTarget == null || score > bestTargetScore)
                {
                    bestTarget = target;
                    bestTargetScore = score;
                }

                objectsByScore.Add(new CombatScoreItem {Target = target, Score = score});
            }

            if (objectsByScore.Count == 0)
            {
                return BestTargetResult.Ok;                
            }

            // Shuffle to get some randomization in the attack order. We pass the battleId as a seed in order to 
            // make it so the attacker doesn't switch targets once it starts attacking them but this way
            // they won't attack the stacks in the order they joined the battle, which usually would mean
            // they will attack the same type of units one after another
            // then sort by score descending
            var shuffled = objectsByScore.Shuffle((int)battleId);
            shuffled.Sort(new CombatScoreItemComparer(attacker, radiusLocator));
            
            var numberOfTargetsToHit = Math.Min(maxCount, objectsByScore.Count);
 
            // Get top results specified by the maxCount param
            result = shuffled.Take(numberOfTargetsToHit).Select(scoreItem => scoreItem.Target).ToList();

            return BestTargetResult.Ok;
        }

        public IEnumerable<ICombatObject> AllCombatObjects()
        {
            return BackingList.SelectMany(group => group.Select(combatObject => combatObject));
        }

        public IEnumerable<ICombatObject> AllAliveCombatObjects()
        {
            return
                    BackingList.SelectMany(
                                           group =>
                                           group.Where(combatObject => !combatObject.IsDead)
                                                .Select(combatObject => combatObject));
        }

        #region Nested type: CombatScoreItem

        private class CombatScoreItem
        {
            public int Score { get; set; }

            public Target Target { get; set; }
        }

        #endregion

        #region Nexted class: CombatComparer
        private class CombatScoreItemComparer : IComparer<CombatScoreItem>
        {
            private readonly ICombatObject attacker;
            private readonly RadiusLocator radiusLocator;

            public CombatScoreItemComparer(ICombatObject attacker, RadiusLocator radiusLocator)
            {
                this.attacker = attacker;
                this.radiusLocator = radiusLocator;
            }

            #region Implementation of IComparer<in CombatScoreItem>
            // return -1 if x is better target, 1 otherwise
            public int Compare(CombatScoreItem x, CombatScoreItem y)
            {
                var xArmorType = x.Target.CombatObject.Stats.Base.Armor;
                var yArmorType = y.Target.CombatObject.Stats.Base.Armor;
                if (x.Score == y.Score && (xArmorType == ArmorType.Building3 || yArmorType == ArmorType.Building3))
                {
                    if (xArmorType == ArmorType.Building3 && yArmorType == ArmorType.Building3)
                    {
                        var xDistance = radiusLocator.RadiusDistance(attacker.Location().X,
                                                                     attacker.Location().Y,
                                                                     x.Target.CombatObject.Location().X,
                                                                     x.Target.CombatObject.Location().Y);
                        var yDistance = radiusLocator.RadiusDistance(attacker.Location().X,
                                                                     attacker.Location().Y,
                                                                     y.Target.CombatObject.Location().X,
                                                                     y.Target.CombatObject.Location().Y);
                        return xDistance.CompareTo(yDistance);
                    }
                    return xArmorType == ArmorType.Building3 ? 1 : -1;
                }

                return x.Score.CompareTo(y.Score) * -1;
            }

            #endregion
        }
        #endregion

        #region Nested type: Target

        public class Target
        {
            public ICombatObject CombatObject { get; set; }

            public ICombatGroup Group { get; set; }
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