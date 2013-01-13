using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Game.Setup;
using Game.Util;

namespace Game.Data.Settlement
{
    class SettlementConfigurator : ISettlementConfigurator
    {
        private readonly NameGenerator nameGenerator = new NameGenerator(Path.Combine(Config.maps_folder, "strongholdnames.txt"));
        private readonly Random random = new Random();
        #region Implementation of ISettlementConfigurator

        public bool Next(out string name, out byte level, out uint x, out uint y)
        {
            x = (uint)random.Next((int)Config.map_width);
            y = (uint)random.Next((int)Config.map_height);
            level = (byte)random.Next(1, 21);
            if (!nameGenerator.Next(out name))
                return false;
            return true;
        }

        #endregion
    }
}
