using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game.Setup;

namespace Game.Map.LocationStrategies
{
    public interface ILocationStrategy
    {
        Error NextLocation(out Position position);
    }
}
