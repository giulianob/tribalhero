namespace Game.Data.Stats {
    public class TroopStats : BaseStats {
        #region Base Stats

        private byte attackRadius = 0;

        public byte AttackRadius {
            get { return attackRadius; }
            set {
                attackRadius = value;
                fireStatsUpdate();
            }
        }

        private byte speed = 0;

        public byte Speed {
            get { return speed; }
            set {
                speed = value;
                fireStatsUpdate();
            }
        }

        private int rewardPoint;

        public int RewardPoint {
            get { return rewardPoint; }
            set {
                rewardPoint = value;
                fireStatsUpdate();
            }
        }

        private Resource loot = new Resource();

        public Resource Loot {
            get { return loot; }
        }

        #endregion

        #region Constructors

        public TroopStats(byte attackRadius, byte speed) {
            this.attackRadius = attackRadius;
            this.speed = speed;

            //We'll listen for loot updates and fire them on this update
            loot.StatsUpdate += new OnStatsUpdate(loot_StatsUpdate);
        }

        #endregion

        #region Events

        private void loot_StatsUpdate() {
            fireStatsUpdate();
        }

        #endregion

        #region ICloneable Members

        public object Clone() {
            return MemberwiseClone();
        }

        #endregion
    }
}