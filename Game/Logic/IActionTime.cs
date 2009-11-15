using System;
using System.Collections.Generic;
using System.Text;

namespace Game.Logic {
    public interface IActionTime {
        DateTime BeginTime { get; }
        DateTime EndTime { get; }
        DateTime NextTime { get; }
    }
}
