namespace Game.Data.Stats
{
    public class TroopStats : BaseStats
    {
        #region Base Stats

        private int attackPoint;
        private byte attackRadius;

        private byte speed;
        private short stamina;

        public byte AttackRadius
        {
            get
            {
                return attackRadius;
            }
            set
            {
                attackRadius = value;
                FireStatsUpdate();
            }
        }

        public byte Speed
        {
            get
            {
                return speed;
            }
            set
            {
                speed = value;
                FireStatsUpdate();
            }
        }

        public int AttackPoint
        {
            get
            {
                return attackPoint;
            }
            set
            {
                attackPoint = value;
                FireStatsUpdate();
            }
        }

        public short Stamina
        {
            get
            {
                return stamina;
            }
            set
            {
                stamina = value;
                FireStatsUpdate();
            }
        }

        public Resource Loot { get; private set; }

        #endregion

        #region Constructors

        public TroopStats(byte attackRadius, byte speed) : this(0, attackRadius, speed, 0, new Resource())
        {
        }

        public TroopStats(int attackPoint, byte attackRadius, byte speed, short stamina, Resource loot)
        {
            AttackPoint = attackPoint;
            Loot = loot;
            Stamina = stamina;
            this.attackRadius = attackRadius;
            this.speed = speed;

            //We'll listen for loot updates and fire them on this update
            Loot.StatsUpdate += LootStatsUpdate;
        }

        #endregion

        #region Events

        private void LootStatsUpdate()
        {
            FireStatsUpdate();
        }

        #endregion
    }
}