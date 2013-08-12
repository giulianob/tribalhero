namespace Game.Battle
{
    public interface IBattleRandom
    {
        int Next(int max);
        
        double NextDouble();

        void UpdateSeed(uint round, uint turn);
    }
}
