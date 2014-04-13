using Game.Data;

namespace Game.Logic.Notifications
{
    public interface INotificationOwner : ILocation
    {
        NotificationManager Notifications { get; }
    }
}