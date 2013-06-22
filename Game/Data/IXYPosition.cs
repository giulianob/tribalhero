using Game.Map;

namespace Game.Data
{
    public interface IXYPosition
    {
        Position PrimaryPosition { get; }

        uint X { get; set; }

        uint Y { get; set; }
    }
}