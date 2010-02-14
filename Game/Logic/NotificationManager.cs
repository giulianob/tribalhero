#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Game.Comm;
using Game.Data;
using Game.Database;

#endregion

namespace Game.Logic {
    public class NotificationManager : IEnumerable<NotificationManager.Notification> {
        private ActionWorker actionWorker;

        private List<Notification> notifications = new List<Notification>();

        private object objLock = new object();

        public class Notification : IEquatable<PassiveAction>, IEquatable<Notification>, IPersistableList {
            private GameObject obj;
            private PassiveAction action;
            private List<City> subscriptions = new List<City>();

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

            private bool dbPersisted = false;

            public bool DbPersisted {
                get { return dbPersisted; }
                set { dbPersisted = value; }
            }

            #endregion

            #region IPersistable Members

            public static readonly string DB_TABLE = "notifications";

            public string DbTable {
                get { return DB_TABLE; }
            }

            public DbColumn[] DbPrimaryKey {
                get {
                    return new DbColumn[] {
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
                get { return new DbColumn[] {new DbColumn("subscription_city_id", DbType.UInt32)}; }
            }

            IEnumerator<DbColumn[]> IEnumerable<DbColumn[]>.GetEnumerator() {
                foreach (City city in subscriptions)
                    yield return new DbColumn[] {new DbColumn("subscription_city_id", city.Id, DbType.UInt32)};
            }

            IEnumerator IEnumerable.GetEnumerator() {
                return subscriptions.GetEnumerator();
            }

            #endregion
        }

        public NotificationManager(ActionWorker worker) {
            actionWorker = worker;

            worker.ActionRescheduled += new ActionWorker.UpdateCallback(worker_ActionRescheduled);
        }

        #region Properties

        public ushort Count {
            get { return (ushort) notifications.Count; }
        }

        #endregion

        public void add(GameObject obj, PassiveAction action, params City[] targetCities) {
            dbLoaderAdd(true, new Notification(obj, action, targetCities));
        }

        public void dbLoaderAdd(bool persist, Notification tmpNotification) {
            lock (objLock) {
                Notification notification =
                    notifications.Find(delegate(Notification other) { return other.Equals(tmpNotification); });

                if (notification == null) {
                    notification = tmpNotification;
                    addNotification(notification);
                }

                foreach (City targetCity in notification.Subscriptions)
                    targetCity.Worker.Notifications.addNotification(notification);

                if (persist)
                    Global.dbManager.Save(notification);
            }
        }

        private void addNotification(Notification notification) {
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

        private void removeNotification(PassiveAction action) {
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

        private void updateNotification(Notification notification) {
            Packet packet = new Packet(Command.NOTIFICATION_UPDATE);
            packet.AddUInt32(actionWorker.City.Id);
            PacketHelper.AddToPacket(notification, packet);
            Global.Channel.Post("/CITY/" + actionWorker.City.Id, packet);
        }

        private void worker_ActionRescheduled(GameAction action) {
            if (!(action is PassiveAction))
                return;

            lock (objLock) {
                Notification notification =
                    notifications.Find(delegate(Notification other) { return other.Equals(action as PassiveAction); });

                if (notification == null)
                    return;

                updateNotification(notification);

                foreach (City city in notification.Subscriptions)
                    city.Worker.Notifications.updateNotification(notification);
            }
        }

        public void remove(PassiveAction action) {
            lock (objLock) {
                Notification notification =
                    notifications.Find(delegate(Notification other) { return other.Equals(action as PassiveAction); });

                if (notification == null)
                    return;

                foreach (City city in notification.Subscriptions)
                    city.Worker.Notifications.removeNotification(action as PassiveAction);

                removeNotification(action as PassiveAction);
                Global.dbManager.Delete(notification);
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

        public bool tryGetValue(City city, ushort actionId, out Notification notification) {
            notification = null;

            foreach (Notification n in notifications) {
                if (n.Action.WorkerObject.City == city && n.Action.ActionId == actionId) {
                    notification = n;
                    return true;
                }
            }

            return false;
        }
    }
}