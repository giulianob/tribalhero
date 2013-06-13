using Game.Data;
using Game.Setup;

namespace Game.Map
{
    public interface IRoadPathFinder
    {
        bool HasPath(Position start, Position end, ICity city, Position excludedPoint);

        Error CanBuild(uint x, uint y, ICity city, bool requiresRoad);
    }
}