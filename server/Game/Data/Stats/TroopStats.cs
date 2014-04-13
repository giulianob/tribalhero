namespace Game.Data.Stats
{
    public class TroopStats : BaseStats
    {
        #region Base Stats

        private int attackPoint;

        private byte attackRadius;

        private decimal speed;

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

        public decimal Speed
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

        public Resource Loot { get; private set; }

        #endregion

        #region Constructors

        public TroopStats(byte attackRadius, decimal speed)
                : this(0, attackRadius, speed, new Resource())
        {
        }

        public TroopStats(int attackPoint, byte attackRadius, decimal speed, Resource loot)
        {
            AttackPoint = attackPoint;
            Loot = loot;
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