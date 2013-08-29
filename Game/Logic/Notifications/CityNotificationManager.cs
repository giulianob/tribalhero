using Game.Comm;
using Game.Data;
using Persistance;

namespace Game.Logic.Notifications
{
    public class CityNotificationManager : NotificationManager
    {
        private readonly string channelName;

        private readonly uint cityId;

        public CityNotificationManager(IActionWorker worker, uint cityId, string channelName, IDbManager dbManager)
                : base(worker, dbManager)
        {
            this.cityId = cityId;
            this.channelName = channelName;
        }

        protected override void RemoveNotification(Notification notification)
        {
            base.RemoveNotification(notification);

            if (Global.Current.FireEvents)
            {
                //send removal
                var packet = new Packet(Command.NotificationRemove);
                packet.AddUInt32(cityId);
                packet.AddUInt32(notification.GameObject.City.Id);
                packet.AddUInt32(notification.Action.ActionId);

                Global.Current.Channel.Post(channelName, packet);
            }
        }

        protected override void UpdateNotification(Notification notification)
        {
            base.UpdateNotification(notification);

            if (Global.Current.FireEvents)
            {
                var packet = new Packet(Command.NotificationUpdate);
                packet.AddUInt32(cityId);
                PacketHelper.AddToPacket(notification, packet);
                Global.Current.Channel.Post(channelName, packet);
            }
        }

        protected override bool AddNotification(Notification notification)
        {
            if (base.AddNotification(notification))
            {
                if (Global.Current.FireEvents)
                {
                    //send add
                    var packet = new Packet(Command.NotificationAdd);
                    packet.AddUInt32(cityId);
                    PacketHelper.AddToPacket(notification, packet);

                    Global.Current.Channel.Post(channelName, packet);
                }

                return true;
            }

            return false;
        }
    }
}