namespace Game.Data.Stronghold
{
    public interface IStrongholdActivationCondition
    {
        bool ShouldActivate(IStronghold stronghold);

        int Score(IStronghold stronghold);
    }
}