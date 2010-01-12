#region

using System.Collections.Generic;

#endregion

namespace Game.Battle {
    class BattleOrder : List<CombatObject> {
        private uint round;

        public BattleOrder(uint round) {
            this.round = round;
        }

        public void ParticipatedInRound() {
            round++;
        }

        public bool NextObject(out CombatObject outObj) {
            outObj = null;

            bool hasMoreInCurrentRound = false;

            foreach (CombatObject obj in this) {
                if (outObj == null && obj.LastRound == round) {
                    outObj = obj;
                    continue;
                }

                if (obj.LastRound == round) {
                    if (outObj != null)
                        return true;
                    else
                        hasMoreInCurrentRound = true;
                }
            }

            if (outObj == null) {
                foreach (CombatObject obj in this) {
                    if (obj.LastRound == (round + 1)) {
                        outObj = obj;
                        break;
                    }
                }
            }

            return hasMoreInCurrentRound;
        }
    }
}