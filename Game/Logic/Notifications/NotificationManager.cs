#region

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game.Data;
using Game.Database;

#endregion

namespace Game.Logic.Notifications
{
    public class NotificationManager : IEnumerable<Notification>
    {
        private readonly List<Notification> notifications = new List<Notification>();

        private readonly object objLock = new object();

        public NotificationManager(IActionWorker worker)
        {
            worker.ActionRescheduled += WorkerActionRescheduled;
            worker.ActionRemoved += WorkerOnActionRemoved;
        }

        private void WorkerOnActionRemoved(GameAction stub, ActionState state)
        {
            PassiveAction passiveAction = stub as PassiveAction;

            if (passiveAction != null)
            {
                Remove(passiveAction);
            }
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

        public void Add(IGameObject obj, PassiveAction action, params INotificationOwner[] targetSubscriptions)
        {
            DbLoaderAdd(true, new Notification(obj, action, targetSubscriptions));
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
            {
                targetCity.Notifications.AddNotification(notification);
            }

            if (persist)
            {
                DbPersistance.Current.Save(notification);
            }
        }

        protected virtual bool AddNotification(Notification notification)
        {
            lock (objLock)
            {
                if (notifications.Contains(notification))
                    return false;

                notifications.Add(notification);

                return true;
            }
        }

        private void RemoveNotificationsForAction(PassiveAction action)
        {
            lock (objLock)
            {
                var notificationsToRemove = notifications.Where(notification => notification.Equals(action)).ToList();

                foreach (var notification in notificationsToRemove) {
                    RemoveNotification(notification);
                }
            }
        }

        protected virtual void RemoveNotification(Notification notification)
        {
            notifications.Remove(notification);
        }

        protected virtual void UpdateNotification(Notification notification)
        {
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
                    city.Notifications.UpdateNotification(notification);
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
            {
                city.Notifications.RemoveNotificationsForAction(action);
            }

            lock (objLock)
            {
                RemoveNotificationsForAction(action);
                DbPersistance.Current.Delete(notification);
            }
        }    

        public bool TryGetValue(ICity city, ushort actionId, out Notification notification)
        {
            notification = notifications.FirstOrDefault(n => n.GameObject.City == city && n.Action.ActionId == actionId);

            return notification != null;
        }
    }
}