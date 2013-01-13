namespace Game.Data.BarbarianTribe
{
    public interface IBarbarianTribeConfigurator
    {
        bool Next(out string name, out byte level, out uint x, out uint y);
    }
}
