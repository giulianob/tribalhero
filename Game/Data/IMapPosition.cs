using System.Collections.Generic;

namespace Game.Map
{
    public interface IMapPosition
    {
        IEnumerable<Position> Positions();

        Position PrimaryPosition { get; }
    }
}