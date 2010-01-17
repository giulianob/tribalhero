namespace Game.Data.Stats {
    //This class just holds the delegate. Just convenient since other stat classes that need an update delegate can just extend it.
    public class BaseStats {
        public delegate void OnStatsUpdate();

        public event OnStatsUpdate StatsUpdate;

        protected void FireStatsUpdate() {
            if (StatsUpdate != null)
                StatsUpdate();
        }
    }
}