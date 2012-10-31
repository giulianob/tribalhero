using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game.Data;

namespace Game.Logic
{
    public interface IActionWorkerOwner: ILocation
    {
        IActionWorker Worker { get; }
    }
}
