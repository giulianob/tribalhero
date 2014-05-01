namespace Game.Map
{
    public interface IRoadManager
    {
        void CreateRoad(uint x, uint y);

        void DestroyRoad(uint x, uint y);

        bool IsRoad(uint x, uint y);
    }
}