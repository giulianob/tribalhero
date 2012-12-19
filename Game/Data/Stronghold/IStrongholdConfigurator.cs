namespace Game.Data.Stronghold
{
    interface IStrongholdConfigurator
    {
        bool Next(out string name, out byte level, out uint x, out uint y);
    }
}