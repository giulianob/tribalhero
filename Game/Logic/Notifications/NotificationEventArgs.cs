using System;

namespace Game.Logic.Notifications
{
    public class NotificationEventArgs : EventArgs
    {
        public Notification Notification { get; set; }

        public NotificationEventArgs(Notification notification)
        {
            Notification = notification;
        }
    }
}