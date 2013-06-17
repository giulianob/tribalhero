using System.Collections.Generic;
using Game.Setup;
using Game.Util;

namespace Game.Data
{
    public interface IGlobal
    {
        IChannel Channel { get; }

        Dictionary<string, SystemVariable> SystemVariables { get; }

        bool FireEvents { get; set; }

        bool IsRunningOnMono();
    }
}