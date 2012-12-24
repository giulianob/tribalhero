namespace Game.Data.Stronghold
{
    interface IStrongholdConfigurator
    {
        bool Next(int index, int count, out string name, out byte level, out uint x, out uint y);
    }
}