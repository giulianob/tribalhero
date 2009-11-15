using System;
using System.Collections.Generic;
using System.Text;

namespace Game.Logic {
    public interface ICanDo {
        City City { get; }
        uint WorkerId { get; }
    }
}
