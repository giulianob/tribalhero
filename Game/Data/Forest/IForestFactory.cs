namespace Game.Data.Forest
{
    public interface IForestFactory
    {
        IForest CreateForest(uint id, byte lvl, int capacity, double rate, uint x, uint y);
    }
}