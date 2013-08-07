using System;

namespace Game.Battle
{
    public class BattleRandom : IBattleRandom
    {
        private uint round;
        private uint turn;
        private readonly uint battleId;

        private bool seedChanged;
        private Random random;

        public BattleRandom(uint battleId)
        {
            this.battleId = battleId;
        }

        private void CheckRandom()
        {
            if (random != null && !seedChanged)
                return;
            seedChanged = false;
            random = new Random((int)((battleId + round) * turn));
        }

        public int Next(int max)
        {
            CheckRandom();
            return random.Next(max);
        }

        public int Next(int min, int max)
        {
            CheckRandom();
            return random.Next(min, max);
        }

        public double NextDouble()
        {
            CheckRandom();
            return random.NextDouble();
        }

        public void UpdateSeed(uint round, uint turn)
        {
            if (this.round == round && this.turn == turn)
                return;
            this.round = round;
            this.turn = turn;
            this.seedChanged = true;
        }

    }
}
