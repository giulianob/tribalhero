#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Game.Comm;
using Game.Data;
using Game.Database;

#endregion

namespace Game.Logic {
    public class NotificationManager : IEnumerable<NotificationManager.Notification> {
        private readonly ActionWorker actionWorker;
        private readonly List<Notification> notifications = new List<Notification>();
        private readonly object objLock = new object();

        public class Notification : IEquatable<PassiveAction>, IEquatable<Notification>, IPersistableList {
            private readonly GameObject obj;
            private readonly PassiveAction action;
            private readonly List<City> subscriptions = new List<City>();

            #region Properties

            public List<City> Subscriptions {
                get { return subscriptions; }
            }

            public PassiveAction Action {
                get { return action; }
            }

            public GameObject GameObject {
                get { return obj; }
            }

            #endregion

            public Notification(GameObject obj, PassiveAction action, params City[] subscriptions) {
                DbPersisted = false;
                if (obj.City != action.WorkerObject.City)
                    throw new Exception("Object should be in the same city as the action worker");
                this.obj = obj;
                this.action = action;
                this.subscriptions.AddRange(subscriptions);
            }

            public bool Equals(PassiveAction other) {
                return other == action;
            }

            public bool Equals(Notification other) {
                return obj == other.obj && action == other.action;
            }

            #region IPersistableObject Members

            public bool DbPersisted { get; set; }

            #endregion

            #region IPersistable Members

            public static readonly string DB_TABLE = "notifications";

            public string DbTable {
                get { return DB_TABLE; }
            }

            public DbColumn[] DbPrimaryKey {
                get {
                    return new[] {
                                              new DbColumn("city_id", obj.City.Id, DbType.UInt32),
                                              new DbColumn("object_id", obj.ObjectId, DbType.UInt32),
                                              new DbColumn("action_id", action.ActionId, DbType.UInt16),
                                          };
                }
            }

            public DbDependency[] DbDependencies {
                get { return new DbDependency[] {}; }
            }

            public DbColumn[] DbColumns {
                get { return new DbColumn[] {}; }
            }

            #endregion

            #region IPersistableList Members

            public DbColumn[] DbListColumns {
                get { return new[] {new DbColumn("subscription_city_id", DbType.UInt32)}; }
            }

            IEnumerator<DbColumn[]> IEnumerable<DbColumn[]>.GetEnumerator() {
                foreach (City city in subscriptions)
                    yield return new[] {new DbColumn("subscription_city_id", city.Id, DbType.UInt32)};
            }

            IEnumerator IEnumerable.GetEnumerator() {
                return subscriptions.GetEnumerator();
            }

            #endregion
        }

        public NotificationManager(ActionWorker worker) {
            actionWorker = worker;

            worker.ActionRescheduled += WorkerActionRescheduled;
        }

        #region Properties

        public ushort Count {
            get { return (ushort) notifications.Count; }
        }

        #endregion

        public void Add(GameObject obj, PassiveAction action, params City[] targetCities) {
            DbLoaderAdd(true, new Notification(obj, action, targetCities));
        }

        public void DbLoaderAdd(bool persist, Notification tmpNotification) {
            lock (objLock) {
                Notification notification =
                    notifications.Find(other => other.Equals(tmpNotification));

                if (notification == null) {
                    notification = tmpNotification;
                    AddNotification(notification);
                }

                foreach (City targetCity in notification.Subscriptions)
                    targetCity.Worker.Notifications.AddNotification(notification);

                if (persist)
                    Global.DbManager.Save(notification);
            }
        }

        private void AddNotification(Notification notification) {
            lock (objLock) {
                if (notifications.Contains(notification))
                    return;

                notifications.Add(notification);

                //send add
                Packet packet = new Packet(Command.NOTIFICATION_ADD);
                packet.AddUInt32(actionWorker.City.Id);
                PacketHelper.AddToPacket(notification, packet);

                Global.Channel.Post("/CITY/" + actionWorker.City.Id, packet);
            }
        }

        private void RemoveNotification(PassiveAction action) {
            lock (objLock) {
                for (int i = notifications.Count - 1; i >= 0; i--) {
                    Notification notification = notifications[i];
                    if (!notification.Equals(action))
                        continue;
                    notifications.RemoveAt(i);

                    //send removal
                    Packet packet = new Packet(Command.NOTIFICATION_REMOVE);
                    packet.AddUInt32(actionWorker.City.Id);
                    packet.AddUInt32(notification.Action.WorkerObject.City.Id);
                    packet.AddUInt16(notification.Action.ActionId);

                    Global.Channel.Post("/CITY/" + actionWorker.City.Id, packet);
                }
            }
        }

        private void UpdateNotification(Notification notification) {
            Packet packet = new Packet(Command.NOTIFICATION_UPDATE);
            packet.AddUInt32(actionWorker.City.Id);
            PacketHelper.AddToPacket(notification, packet);
            Global.Channel.Post("/CITY/" + actionWorker.City.Id, packet);
        }

        private void WorkerActionRescheduled(GameAction action) {
            if (!(action is PassiveAction))
                return;

            lock (objLock) {
                Notification notification =
                    notifications.Find(other => other.Equals(action as PassiveAction));

                if (notification == null)
                    return;

                UpdateNotification(notification);

                foreach (City city in notification.Subscriptions)
                    city.Worker.Notifications.UpdateNotification(notification);
            }
        }

        public void Remove(PassiveAction action) {
            lock (objLock) {
                Notification notification =
                    notifications.Find(other => other.Equals(action));

                if (notification == null)
                    return;

                foreach (City city in notification.Subscriptions)
                    city.Worker.Notifications.RemoveNotification(action);

                RemoveNotification(action);
                Global.DbManager.Delete(notification);
            }
        }

        #region IEnumerable<Notification> Members

        public IEnumerator<Notification> GetEnumerator() {
            return notifications.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator() {
            return notifications.GetEnumerator();
        }

        #endregion

        public bool TryGetValue(City city, ushort actionId, out Notification notification) {
            notification = notifications.FirstOrDefault(n => n.Action.WorkerObject.City == city && n.Action.ActionId == actionId);

            return notification != null;
        }
    }
}