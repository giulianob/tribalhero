#region

using System;
using Game.Data;
using Game.Database;
using Game.Map;
using Game.Map.LocationStrategies;
using Game.Setup;
using Ninject;

#endregion

namespace Game.Logic
{
    public class Randomizer
    {
        public static Random Random = Config.Random;

        public static uint MapX
        {
            get
            {
                return (uint)Random.Next(Config.width_margin, 50 - Config.width_margin);
            }
        }

        public static uint MapY
        {
            get
            {
                return (uint)Random.Next(Config.height_margin, 50 - Config.height_margin);
            }
        }
        
        public static Error MainBuilding(ICity city, ILocationStrategy strategy, byte lvl, out IStructure structure)
        {            
            structure = city.CreateStructure(2000, lvl);
            Position position;
            var error = strategy.NextLocation(out position);
            if(error != Error.Ok)
            {
                structure = null;
                return error;
            }
            structure.X = position.X;
            structure.Y = position.Y;
            return Error.Ok;
        }
    }
}