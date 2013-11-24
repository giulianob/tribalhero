using System;

namespace Game.Data.Forest
{
    public interface IForestFactory
    {
        IForest CreateForest(int capacity);
    }
}