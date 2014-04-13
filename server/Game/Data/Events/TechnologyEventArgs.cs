using System;

namespace Game.Data.Events
{
    public class TechnologyEventArgs : EventArgs
    {
        public ITechnologyManager TechnologyManager { get; set; }

        public Technology Technology { get; set; }
    }
}
