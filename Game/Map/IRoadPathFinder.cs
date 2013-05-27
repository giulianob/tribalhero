using Game.Data;

namespace Game.Map
{
    public interface IRoadPathFinder
    {
        bool HasPath(Position start, Position end, ICity city, Position excludedPoint);
    }
}