namespace Game.Logic.Notifications
{
    public interface INotificationManagerFactory
    {
        CityNotificationManager CreateCityNotificationManager(IActionWorker worker, uint cityId, string channelName);

        NotificationManager CreateNotificationManager(IActionWorker worker);
    }
}