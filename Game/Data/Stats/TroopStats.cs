namespace Game.Data.Stats {
    public class TroopStats : BaseStats {
        #region Base Stats

        private byte attackRadius;

        public byte AttackRadius {
            get { return attackRadius; }
            set {
                attackRadius = value;
                FireStatsUpdate();
            }
        }

        private byte speed;

        public byte Speed {
            get { return speed; }
            set {
                speed = value;
                FireStatsUpdate();
            }
        }

        private int rewardPoint;

        public int RewardPoint {
            get { return rewardPoint; }
            set {
                rewardPoint = value;
                FireStatsUpdate();
            }
        }

        public Resource Loot { get; private set; }

        #endregion

        #region Constructors

        public TroopStats(byte attackRadius, byte speed) {
            Loot = new Resource();
            this.attackRadius = attackRadius;
            this.speed = speed;

            //We'll listen for loot updates and fire them on this update
            Loot.StatsUpdate += LootStatsUpdate;
        }

        #endregion

        #region Events

        private void LootStatsUpdate() {
            FireStatsUpdate();
        }

        #endregion
    }
}