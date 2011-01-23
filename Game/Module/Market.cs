#region

using System;
using System.Data;
using Game.Data;
using Game.Database;
using Game.Logic;
using Game.Setup;

#endregion

namespace Game.Module
{
    public class Market : ISchedule, IPersistableObject
    {
        private const int UPDATE_INTERVAL_IN_SECOND = 3600;

        private const int MIN_PRICE = 5;
        private const int MAX_PRICE = 1000;

        private const int QUANTITY_PER_CHANGE_PER_PLAYER = 200;
        public const string DB_TABLE = "market";

        private static Market crop;
        private static Market iron;
        private static Market wood;
        private readonly object marketLock = new object();
        private readonly int quantityPerChangePerPlayer;
        private readonly ResourceType resource;

        private int incoming;
        private int outgoing;
        private int price;
        private DateTime time;

        public Market(ResourceType resource, int defaultPrice)
        {
            price = defaultPrice;
            quantityPerChangePerPlayer = QUANTITY_PER_CHANGE_PER_PLAYER;
            time = DateTime.UtcNow.AddSeconds(UPDATE_INTERVAL_IN_SECOND*Config.seconds_per_unit);
            this.resource = resource;
            Global.Scheduler.Put(this);
        }

        public static Market Crop
        {
            get
            {
                return crop;
            }
            set
            {
                crop = value;
            }
        }

        public static Market Iron
        {
            get
            {
                return iron;
            }
            set
            {
                iron = value;
            }
        }

        public static Market Wood
        {
            get
            {
                return wood;
            }
            set
            {
                wood = value;
            }
        }

        public int Price
        {
            get
            {
                return price;
            }
        }

        #region IPersistableObject Members

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
                return new[] {new DbColumn("resource_type", (byte)resource, DbType.Byte)};
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
                return new[]
                       {
                               new DbColumn("incoming", incoming, DbType.Int32), new DbColumn("outgoing", outgoing, DbType.Int32),
                               new DbColumn("price", price, DbType.Int32), new DbColumn("quantity_per_change", quantityPerChangePerPlayer, DbType.Int32)
                       };
            }
        }

        #endregion

        #region ISchedule Members

        public bool IsScheduled { get; set; }

        public DateTime Time
        {
            get
            {
                return time;
            }
        }

        public void Callback(object custom)
        {
            lock (marketLock)
            {
                using (Global.DbManager.GetThreadTransaction())
                {
                    int flow = outgoing - incoming;
                    if (Global.World.Players.Count > 0)
                    {
                        price += (flow/(quantityPerChangePerPlayer*Global.World.Players.Count));
                        if (price < MIN_PRICE)
                            price = MIN_PRICE;
                        if (price > MAX_PRICE)
                            price = MAX_PRICE;
                        outgoing = incoming = 0;
                    }
                    time = DateTime.UtcNow.AddSeconds(UPDATE_INTERVAL_IN_SECOND*Config.seconds_per_unit);
                    Global.DbManager.Save(this);
                    Global.Scheduler.Put(this);
                }
            }
        }

        #endregion

        public static void Init()
        {
            if (crop == null)
                crop = new Market(ResourceType.Crop, 50);

            if (iron == null)
                iron = new Market(ResourceType.Iron, 500);

            if (wood == null)
                wood = new Market(ResourceType.Wood, 50);
        }

        public void DbLoad(int outgoing, int incoming)
        {
            this.outgoing = outgoing;
            this.incoming = incoming;
        }

        public bool Buy(int quantity, int price)
        {
            lock (marketLock)
            {
                if (price != Price)
                    return false;
                outgoing += quantity;
                return true;
            }
        }

        public bool Sell(int quantity, int price)
        {
            lock (marketLock)
            {
                if (price != Price)
                    return false;
                incoming += quantity;
                return true;
            }
        }

        public void Supply(ushort quantity)
        {
            lock (marketLock)
            {
                incoming += quantity;
            }
        }

        public void Consume(ushort quantity)
        {
            lock (marketLock)
            {
                outgoing += quantity;
            }
        }
    }
}