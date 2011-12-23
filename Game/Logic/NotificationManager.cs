#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Game.Comm;
using Game.Data;
using Game.Database;
using Game.Setup;
using Game.Util;
using Ninject;
using Persistance;

#endregion

namespace Game.Logic
{
    public class NotificationManager : IEnumerable<NotificationManager.Notification>
    {
        private readonly ActionWorker actionWorker;
        private readonly List<Notification> notifications = new List<Notification>();
        private readonly object objLock = new object();

        public NotificationManager(ActionWorker worker)
        {
            actionWorker = worker;

            worker.ActionRescheduled += WorkerActionRescheduled;
        }

        #region Properties

        public ushort Count
        {
            get
            {
                return (ushort)notifications.Count;
            }
        }

        #endregion

        #region IEnumerable<Notification> Members

        public IEnumerator<Notification> GetEnumerator()
        {
            return notifications.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return notifications.GetEnumerator();
        }

        #endregion

        public void Add(GameObject obj, PassiveAction action, params ICity[] targetCities)
        {
            DbLoaderAdd(true, new Notification(obj, action, targetCities));
        }

        public void DbLoaderAdd(bool persist, Notification tmpNotification)
        {
            Notification notification;

            lock (objLock)
            {
                notification = notifications.Find(other => other.Equals(tmpNotification));

                if (notification == null)
                {
                    notification = tmpNotification;
                    AddNotification(notification);
                }
            }

            foreach (var targetCity in notification.Subscriptions)
                targetCity.Worker.Notifications.AddNotification(notification);

            if (persist)
                DbPersistance.Current.Save(notification);
        }

        private void AddNotification(Notification notification)
        {
            lock (objLock)
            {
                if (notifications.Contains(notification))
                    return;

                notifications.Add(notification);

                if (Global.FireEvents)
                {
                    //send add
                    var packet = new Packet(Command.NotificationAdd);
                    packet.AddUInt32(actionWorker.City.Id);
                    PacketHelper.AddToPacket(notification, packet);

                    Global.Channel.Post("/CITY/" + actionWorker.City.Id, packet);
                }
            }
        }

        private void RemoveNotification(PassiveAction action)
        {
            lock (objLock)
            {
                for (int i = notifications.Count - 1; i >= 0; i--)
                {
                    Notification notification = notifications[i];
                    if (!notification.Equals(action))
                        continue;
                    notifications.RemoveAt(i);

                    if (Global.FireEvents)
                    {
                        //send removal
                        var packet = new Packet(Command.NotificationRemove);
                        packet.AddUInt32(actionWorker.City.Id);
                        packet.AddUInt32(notification.Action.WorkerObject.City.Id);
                        packet.AddUInt32(notification.Action.ActionId);

                        Global.Channel.Post("/CITY/" + actionWorker.City.Id, packet);
                    }
                }
            }
        }

        private void UpdateNotification(Notification notification)
        {
            if (Global.FireEvents)
            {
                var packet = new Packet(Command.NotificationUpdate);
                packet.AddUInt32(actionWorker.City.Id);
                PacketHelper.AddToPacket(notification, packet);
                Global.Channel.Post("/CITY/" + actionWorker.City.Id, packet);
            }
        }

        private void WorkerActionRescheduled(GameAction action, ActionState state)
        {
            if (!(action is PassiveAction))
                return;

            lock (objLock)
            {
                Notification notification = notifications.Find(other => other.Equals(action as PassiveAction));

                if (notification == null)
                    return;

                UpdateNotification(notification);

                foreach (var city in notification.Subscriptions)
                    city.Worker.Notifications.UpdateNotification(notification);
            }
        }

        public void Remove(PassiveAction action)
        {
            Notification notification;
            lock (objLock)
            {
                notification = notifications.Find(other => other.Equals(action));

                if (notification == null)
                    return;

            }

            foreach (var city in notification.Subscriptions)
                    city.Worker.Notifications.RemoveNotification(action);

            lock (objLock)
            {
                RemoveNotification(action);
                DbPersistance.Current.Delete(notification);
            }
        }    

        public bool TryGetValue(ICity city, ushort actionId, out Notification notification)
        {
            notification = notifications.FirstOrDefault(n => n.Action.WorkerObject.City == city && n.Action.ActionId == actionId);

            return notification != null;
        }

        #region Nested type: Notification

        public class Notification : IEquatable<PassiveAction>, IEquatable<Notification>, IPersistableList
        {
            public const string DB_TABLE = "notifications";
            private readonly PassiveAction action;
            private readonly GameObject obj;
            private readonly List<ICity> subscriptions = new List<ICity>();

            #region Properties

            public List<ICity> Subscriptions
            {
                get
                {
                    return subscriptions;
                }
            }

            public PassiveAction Action
            {
                get
                {
                    return action;
                }
            }

            public GameObject GameObject
            {
                get
                {
                    return obj;
                }
            }

            #endregion

            public Notification(GameObject obj, PassiveAction action, params ICity[] subscriptions)
            {
                DbPersisted = false;
                if (obj.City != action.WorkerObject.City)
                    throw new Exception("Object should be in the same city as the action worker");
                this.obj = obj;
                this.action = action;
                this.subscriptions.AddRange(subscriptions);
            }

            #region IEquatable<Notification> Members

            public bool Equals(Notification other)
            {
                return obj == other.obj && action == other.action;
            }

            #endregion

            #region IEquatable<PassiveAction> Members

            public bool Equals(PassiveAction other)
            {
                return other == action;
            }

            #endregion

            #region IPersistableList Members

            public bool DbPersisted { get; set; }

            public string DbTable
            {
                get
                {
                    return DB_TABLE;
                }
            }

            public DbColumn[] DbPrimaryKey
            {
                get
                {
                    return new[]
                           {
                                   new DbColumn("city_id", obj.City.Id, DbType.UInt32), new DbColumn("object_id", obj.ObjectId, DbType.UInt32),
                                   new DbColumn("action_id", action.ActionId, DbType.UInt32),
                           };
                }
            }

            public DbDependency[] DbDependencies
            {
                get
                {
                    return new DbDependency[] {};
                }
            }

            public DbColumn[] DbColumns
            {
                get
                {
                    return new DbColumn[] {};
                }
            }

            public DbColumn[] DbListColumns
            {
                get
                {
                    return new[] {new DbColumn("subscription_city_id", DbType.UInt32)};
                }
            }

            public IEnumerable<DbColumn[]> DbListValues()
            {
                return subscriptions.Select(city => new[] {new DbColumn("subscription_city_id", city.Id, DbType.UInt32)});
            }

            #endregion
        }

        #endregion
    }
}