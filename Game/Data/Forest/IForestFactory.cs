using System;

namespace Game.Data.Forest
{
    public interface IForestFactory
    {
        IForest CreateForest(byte lvl, int capacity, double rate);
    }
}