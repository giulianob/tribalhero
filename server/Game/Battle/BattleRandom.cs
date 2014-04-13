using System;

namespace Game.Battle
{
    public class BattleRandom : IBattleRandom
    {
        private uint round;
        private uint turn;
        private readonly uint battleId;

        private bool seedChanged = true;
        private Random random;

        public BattleRandom(uint battleId)
        {
            this.battleId = battleId;
        }

        private void CheckRandom()
        {
            if (!seedChanged)
            {
                return;
            }

            seedChanged = false;

            int hash;
            unchecked
            {
                hash = (int)(battleId + (turn * 397) ^ round);
            }

            random = new Random(hash);
        }

        public int Next(int max)
        {
            CheckRandom();
            return random.Next(max);
        }

        public double NextDouble()
        {
            CheckRandom();
            return random.NextDouble();
        }

        public void UpdateSeed(uint round, uint turn)
        {
            this.round = round;
            this.turn = turn;
            this.seedChanged = true;
        }

    }
}
