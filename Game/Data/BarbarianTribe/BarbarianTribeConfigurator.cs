using System;
using System.IO;
using Game.Setup;
using Game.Util;

namespace Game.Data.BarbarianTribe
{
    class BarbarianTribeConfigurator : IBarbarianTribeConfigurator
    {
        private readonly NameGenerator nameGenerator = new NameGenerator(Path.Combine(Config.maps_folder, "strongholdnames.txt"));
        private readonly Random random = new Random();
        
        public bool Next(out string name, out byte level, out uint x, out uint y)
        {
            x = (uint)random.Next((int)Config.map_width);
            y = (uint)random.Next((int)Config.map_height);
            level = (byte)random.Next(1, 21);
            if (!nameGenerator.Next(out name))
            {
                return false;
            }

            return true;
        }
    }
}
