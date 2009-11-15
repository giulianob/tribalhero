using System;
using System.Collections.Generic;
using System.Text;
using Game.Logic;
using Game.Comm;
using Game.Data;
using Game.Util;
using Game.Database;

namespace Game.Logic {
    public class NotificationManager: IEnumerable<Game.Logic.NotificationManager.Notification> {
        ActionWorker actionWorker;

        List<Notification> notifications = new List<Notification>();

        object objLock = new object();

        public class Notification : IEquatable<PassiveAction>, IEquatable<Notification>, IPersistableList {
            GameObject obj;
            PassiveAction action;
            List<City> subscriptions = new List<City>();

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
                if (obj.City != action.WorkerObject.City) throw new Exception("Object should be in the same city as the action worker");
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

            bool dbPersisted = false;
            public bool DbPersisted {
                get {
                    return dbPersisted;
                }
                set {
                    dbPersisted = value;
                }
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
                        new DbColumn("city_id", obj.City.CityId, System.Data.DbType.UInt32),                        
                        new DbColumn("object_id", obj.ObjectID, System.Data.DbType.UInt32),
                        new DbColumn("action_id", action.ActionId, System.Data.DbType.UInt16),                     
                    };
                }
            }

            public DbDependency[] DbDependencies {
                get { return new DbDependency[] { }; }
            }

            public DbColumn[] DbColumns {
                get { return new DbColumn[] { }; }
            }

            #endregion

            #region IPersistableList Members
            
            public DbColumn[] DbListColumns {
                get { return new DbColumn[] { 
                        new DbColumn("subscription_city_id", System.Data.DbType.UInt32)
                    }; 
                }
            }

            IEnumerator<DbColumn[]> IEnumerable<DbColumn[]>.GetEnumerator() {                
                foreach (City city in subscriptions) {
                    yield return new DbColumn[] { 
                        new DbColumn("subscription_city_id", city.CityId, System.Data.DbType.UInt32)
                    };
                }
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
                return subscriptions.GetEnumerator();
            }
            
            #endregion
        }
        
        public NotificationManager(ActionWorker worker) {
            this.actionWorker = worker;

            worker.ActionRescheduled += new ActionWorker.UpdateCallback(worker_ActionRescheduled);
        }

        #region Properties
        public ushort Count {
            get {
                return (ushort)notifications.Count;
            }
        }
        #endregion

        public void add(GameObject obj, PassiveAction action, params City[] targetCities) {
            dbLoaderAdd(true, new Notification(obj, action, targetCities));
        }

        public void dbLoaderAdd(bool persist, Notification tmpNotification) {
            lock (objLock) {               

                Notification notification = notifications.Find(delegate(Notification other) {
                    return other.Equals(tmpNotification);
                });

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

        void addNotification(Notification notification) {
            lock (objLock) {
                if (notifications.Contains(notification)) return;

                notifications.Add(notification);

                //send add
                Packet packet = new Packet(Command.NOTIFICATION_ADD);
                packet.addUInt32(actionWorker.City.CityId);
                PacketHelper.AddToPacket(notification, packet);

                actionWorker.City.Channel.post(packet);
            }
        }

        void removeNotification(PassiveAction action) {
            lock (objLock) {
                for (int i = notifications.Count - 1; i >= 0; i--) {
                    Notification notification = notifications[i];
                    if (!notification.Equals(action)) continue;
                    notifications.RemoveAt(i);

                    //send removal
                    Packet packet = new Packet(Command.NOTIFICATION_REMOVE);
                    packet.addUInt32(actionWorker.City.CityId);
                    packet.addUInt32(notification.Action.WorkerObject.City.CityId);
                    packet.addUInt16(notification.Action.ActionId);

                    actionWorker.City.Channel.post(packet);
                }                
            }
        }

        void updateNotification(Notification notification) {
            Packet packet = new Packet(Command.NOTIFICATION_UPDATE);
            packet.addUInt32(actionWorker.City.CityId);
            PacketHelper.AddToPacket(notification, packet);
            actionWorker.City.Channel.post(packet);
        }

        void worker_ActionRescheduled(Game.Logic.Action action) {
            if (!(action is PassiveAction)) return;

            lock (objLock) {

                Notification notification = notifications.Find(delegate(Notification other) {
                    return other.Equals(action as PassiveAction);
                });

                if (notification == null) return;

                updateNotification(notification);

                foreach (City city in notification.Subscriptions) {
                    city.Worker.Notifications.updateNotification(notification);
                }
            }
        }

        public void remove(PassiveAction action) {
            lock (objLock) {
                Notification notification = notifications.Find(delegate(Notification other) {
                    return other.Equals(action as PassiveAction);
                });

                if (notification == null) return;

                foreach (City city in notification.Subscriptions) {
                    city.Worker.Notifications.removeNotification(action as PassiveAction);
                }
                
                removeNotification(action as PassiveAction);
                Global.dbManager.Delete(notification);
            }
        }



        #region IEnumerable<Notification> Members

        public IEnumerator<NotificationManager.Notification> GetEnumerator() {
            return notifications.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
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
