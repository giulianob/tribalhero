using Game.Data;
using Game.Setup;

namespace Game.Map
{
    public class RegionLocator : IRegionLocator
    {
        public ushort GetRegionIndex(ISimpleGameObject obj)
        {
            return GetRegionIndex(obj.X, obj.Y);
        }

        public ushort GetRegionIndex(uint x, uint y)
        {
            return (ushort)(x / Config.region_width + (y / Config.region_height) * (int)(Config.map_width / Config.region_width));
        }

        public int GetTileIndex(uint x, uint y)
        {
            return (int)(x % Config.region_width + (y % Config.region_height) * Config.region_width);
        }
    }
}
