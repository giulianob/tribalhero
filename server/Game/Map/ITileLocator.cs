using System.Collections.Generic;
using Game.Data;

namespace Game.Map
{
    public interface ITileLocator
    {
        int RadiusDistance(Position position, byte size, Position position1, byte size1);

        bool IsOverlapping(Position location1, byte size1, byte r1, Position location2, byte size2, byte r2);

        int TileDistance(Position position, byte size1, Position position1, byte size2);

        void RandomPoint(Position position, byte radius, bool doSelf, out Position randomPosition);

        IEnumerable<Position> ForeachMultitile(ISimpleGameObject obj);

        IEnumerable<Position> ForeachMultitile(uint ox, uint oy, byte size);

        IEnumerable<Position> ForeachTile(uint ox, uint oy, int radius, bool includeCenter  = true);

        IEnumerable<Position> ForeachRadius(uint ox, uint oy, byte radius, bool includeCenter = true);

        IEnumerable<Position> ForeachRadius(uint ox, uint oy, byte size, byte radius);

        int TileDistance(ISimpleGameObject obj1, ISimpleGameObject obj2);        
    }
}