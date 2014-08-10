using Game.Data;

namespace Game.Map
{
    public interface IRoadManager
    {
        void CreateRoad(uint x, uint y, string themeId);

        void DestroyRoad(uint x, uint y, string themeId);

        bool IsRoad(uint x, uint y);

        void ChangeRoadTheme(ICity city, string oldTheme, string newTheme);
    }
}