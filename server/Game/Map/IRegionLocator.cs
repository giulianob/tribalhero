using Game.Data;

namespace Game.Map
{
    public interface IRegionLocator
    {
        ushort GetRegionIndex(ISimpleGameObject obj);

        ushort GetRegionIndex(uint x, uint y);

        int GetTileIndex(uint x, uint y);
    }
}