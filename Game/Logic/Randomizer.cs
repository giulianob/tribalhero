#region

using Game.Data;
using Game.Map;
using Game.Map.LocationStrategies;
using Game.Setup;
using Ninject;

#endregion

namespace Game.Logic
{
    public class Randomizer
    {
        public static Error MainBuilding(out IStructure structure, ILocationStrategy strategy, byte lvl)
        {
            structure = Ioc.Kernel.Get<StructureFactory>().GetNewStructure(2000, lvl);
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