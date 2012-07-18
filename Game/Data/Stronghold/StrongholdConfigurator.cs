using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game.Setup;
using Game.Util;

namespace Game.Data.Stronghold
{
    class StrongholdConfigurator : IStrongholdConfigurator
    {
        private readonly NameGenerator nameGenerator = new NameGenerator(Config.stronghold_name_txt_file);

        #region Implementation of IStrongholdConfigurator

        public bool Next(out string name, out byte level, out uint x, out uint y)
        {
            if (!nameGenerator.Next(out name))
            {
                name = "";
                level = 0;
                x = 0;
                y = 0;
                return false;
            }
            level = (byte)(Config.Random.Next(20) + 1);
            x = (uint)Config.Random.Next((int)Config.map_width);
            y = (uint)Config.Random.Next((int)Config.map_height);
            return true;
        }

        #endregion
    }
}
