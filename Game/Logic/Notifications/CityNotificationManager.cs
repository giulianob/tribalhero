using Game.Comm;
using Game.Data;
using Persistance;

namespace Game.Logic.Notifications
{
    class CityNotificationManager : NotificationManager
    {
        private readonly string channelName;

        private readonly uint cityId;

        public CityNotificationManager(IActionWorker worker, uint cityId, IDbManager actionWorker)
                : base(worker, actionWorker)
        {
            this.cityId = cityId;
            channelName = "/CITY/" + cityId;
        }

        protected override void RemoveNotification(Notification notification)
        {
            base.RemoveNotification(notification);

            if (Global.FireEvents)
            {
                //send removal
                var packet = new Packet(Command.NotificationRemove);
                packet.AddUInt32(cityId);
                packet.AddUInt32(notification.GameObject.City.Id);
                packet.AddUInt32(notification.Action.ActionId);

                Global.Channel.Post(channelName, packet);
            }
        }

        protected override void UpdateNotification(Notification notification)
        {
            base.UpdateNotification(notification);

            if (Global.FireEvents)
            {
                var packet = new Packet(Command.NotificationUpdate);
                packet.AddUInt32(cityId);
                PacketHelper.AddToPacket(notification, packet);
                Global.Channel.Post(channelName, packet);
            }
        }

        protected override bool AddNotification(Notification notification)
        {
            if (base.AddNotification(notification))
            {
                if (Global.FireEvents)
                {
                    //send add
                    var packet = new Packet(Command.NotificationAdd);
                    packet.AddUInt32(cityId);
                    PacketHelper.AddToPacket(notification, packet);

                    Global.Channel.Post(channelName, packet);
                }

                return true;
            }

            return false;
        }
    }
}