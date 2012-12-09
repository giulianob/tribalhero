namespace Game.Data.Stronghold
{
    interface IStrongholdActivationCondition
    {
        bool ShouldActivate(IStronghold stronghold);
    }
}