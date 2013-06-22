using System.Collections.Generic;

namespace Game.Map
{
    public interface ITileLocator
    {
        int RadiusDistance(uint x, uint y, uint x1, uint y1);

        bool IsOverlapping(Position location1, byte r1, Position location2, byte r2);

        int TileDistance(uint x, uint y, uint x1, uint y1);

        void RandomPoint(uint ox, uint oy, byte radius, bool doSelf, out uint x, out uint y);

        IEnumerable<Position> ForeachMultitile(uint ox, uint oy, byte size);

        IEnumerable<Position> ForeachTile(uint ox, uint oy, int radius, bool includeCenter  = true);

        IEnumerable<Position> ForeachRadius(uint ox, uint oy, byte radius, bool includeCenter = true);
    }
}