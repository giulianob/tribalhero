namespace Game.Data.Stronghold
{
    class DummyActivationCondition : IStrongholdActivationCondition
    {
        #region Implementation of IStrongholdActivationCondition

        public bool ShouldActivate(IStronghold stronghold)
        {
            return true;
        }

        public int Score(IStronghold stronghold)
        {
            return 0;
        }

        #endregion
    }
}