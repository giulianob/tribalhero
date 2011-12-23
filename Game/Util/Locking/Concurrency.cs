using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game.Util.Locking
{
    public static class Concurrency
    {
        public static ILocker Current { get; set; }
    }
}
