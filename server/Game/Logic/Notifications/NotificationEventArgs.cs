using System;

namespace Game.Logic.Notifications
{
    public class NotificationEventArgs : EventArgs
    {
        public NotificationEventArgs(Notification notification)
        {
            Notification = notification;
        }

        public Notification Notification { get; set; }
    }
}