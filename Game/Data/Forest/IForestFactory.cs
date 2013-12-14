namespace Game.Data.Forest
{
    public interface IForestFactory
    {
        IForest CreateForest(uint id, int capacity, uint x, uint y);
    }
}