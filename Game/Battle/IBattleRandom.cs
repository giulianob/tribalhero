using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game.Battle
{
    public interface IBattleRandom
    {
        int Next(int max);
        int Next(int min, int max);
        double NextDouble();
        void UpdateSeed(uint round, uint turn);
    }
}
