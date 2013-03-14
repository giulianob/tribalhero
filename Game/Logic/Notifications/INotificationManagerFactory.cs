namespace Game.Logic.Notifications
{
    public interface INotificationManagerFactory
    {
        CityNotificationManager CreateCityNotificationManager(IActionWorker worker, uint cityId);

        NotificationManager CreateNotificationManager(IActionWorker worker);
    }
}