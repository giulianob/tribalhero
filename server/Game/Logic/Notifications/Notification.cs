using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Game.Data;
using Persistance;

namespace Game.Logic.Notifications
{
    public class Notification : IEquatable<PassiveAction>, IEquatable<Notification>, IPersistableList
    {
        public const string DB_TABLE = "notifications";

        private readonly PassiveAction action;

        private readonly IGameObject obj;

        private readonly List<INotificationOwner> subscriptions = new List<INotificationOwner>();

        #region Properties

        public List<INotificationOwner> Subscriptions
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

        public IGameObject GameObject
        {
            get
            {
                return obj;
            }
        }

        #endregion

        public Notification(IGameObject obj, PassiveAction action, params INotificationOwner[] subscriptions)
        {
            DbPersisted = false;
            if (obj.City.Id != action.Location.LocationId || action.Location.LocationType != LocationType.City)
            {
                throw new Exception("Object should be in the same city as the action worker");
            }

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
                        new DbColumn("city_id", obj.City.Id, DbType.UInt32),
                        new DbColumn("object_id", obj.ObjectId, DbType.UInt32),
                        new DbColumn("action_id", action.ActionId, DbType.UInt32),
                };
            }
        }

        public IEnumerable<DbDependency> DbDependencies
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

        public IEnumerable<DbColumn> DbListColumns
        {
            get
            {
                return new[]
                {
                        new DbColumn("subscription_location_type", DbType.String),
                        new DbColumn("subscription_location_id", DbType.UInt32),
                };
            }
        }

        public IEnumerable<DbColumn[]> DbListValues()
        {
            return
                    subscriptions.Select(
                                         subscription =>
                                         new[]
                                         {
                                                 new DbColumn("subscription_location_type",
                                                              subscription.LocationType.ToString(),
                                                              DbType.String),
                                                 new DbColumn("subscription_location_id",
                                                              subscription.LocationId,
                                                              DbType.UInt32)
                                         });
        }

        #endregion
    }
}