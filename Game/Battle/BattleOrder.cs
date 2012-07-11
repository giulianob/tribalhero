#region

using System.Collections.Generic;
using System.Linq;
using Game.Battle.CombatObjects;

#endregion

namespace Game.Battle
{
    class BattleOrder : List<CombatObject>
    {
        private uint round;

        public BattleOrder(uint round)
        {
            this.round = round;
        }

        public void ParticipatedInRound()
        {
            round++;
        }

        public bool NextObject(out CombatObject outObj)
        {
            if (Count == 0)
            {
                outObj = null;
                return true;
            }

            outObj = this.FirstOrDefault(obj => obj.LastRound == round) ?? this.FirstOrDefault(obj => obj.LastRound == round + 1);

            return !TrueForAll(t => t.LastRound == round + 1);
        }
    }
}