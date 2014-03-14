using Game.Setup.DependencyInjection;
using Persistance;

namespace Game.Logic.Notifications
{
    public class NotificationManagerFactory : INotificationManagerFactory
    {
        private readonly IKernel kernel;

        public NotificationManagerFactory(IKernel kernel)
        {
            this.kernel = kernel;
        }

        public CityNotificationManager CreateCityNotificationManager(IActionWorker worker, uint cityId, string channelName)
        {
            return new CityNotificationManager(worker, cityId, channelName, kernel.Get<IDbManager>());
        }

        public NotificationManager CreateNotificationManager(IActionWorker worker)
        {
            return new NotificationManager(worker, kernel.Get<IDbManager>());
        }
    }
}