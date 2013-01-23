namespace Game.Data.BarbarianTribe
{
    public interface IBarbarianTribeConfigurator
    {
        bool Next(int count, out byte level, out uint x, out uint y);
    }
}
