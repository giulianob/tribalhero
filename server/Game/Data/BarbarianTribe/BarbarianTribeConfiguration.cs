using Game.Map;

namespace Game.Data.BarbarianTribe
{
    public class BarbarianTribeConfiguration
    {
        public byte Level { get; set; }

        public Position PrimaryPosition { get; set; }

        public BarbarianTribeConfiguration()
        {
        }

        public BarbarianTribeConfiguration(byte level, Position primaryPosition)
        {
            Level = level;
            PrimaryPosition = primaryPosition;
        }
    }
}