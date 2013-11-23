using System.Collections.Generic;
using Game.Data;
using Game.Setup;

namespace Game.Map
{
    public interface IRoadPathFinder
    {
        bool HasPath(Position start, byte startSize, ICity city, IEnumerable<Position> excludedPoint);

        Error CanBuild(Position position, byte size, ICity city, bool requiresRoad);
    }
}