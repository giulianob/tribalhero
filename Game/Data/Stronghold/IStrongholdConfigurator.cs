namespace Game.Data.Stronghold
{
    public interface IStrongholdConfigurator
    {
        bool Next(int index, int count, out string name, out byte level, out uint x, out uint y);
    }
}