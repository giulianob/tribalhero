using System;

namespace Game.Data.Events
{
    public class NewCityEventArgs : EventArgs
    {
        public bool IsNew { get; set; }

        public NewCityEventArgs(bool isNew)
        {
            IsNew = isNew;
        }
    }
}
