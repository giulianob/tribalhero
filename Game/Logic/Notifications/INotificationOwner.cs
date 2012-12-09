using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game.Data;

namespace Game.Logic.Notifications
{
    public interface INotificationOwner: ILocation
    {
        NotificationManager Notifications { get; }
    }
}
