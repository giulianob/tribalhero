#region

using System;
using System.Data;
using Game.Data;
using Game.Database;
using Game.Logic;
using Game.Setup;

#endregion

namespace Game.Module {
    public class Market : ISchedule, IPersistableObject {
        private readonly object marketLock = new object();

        private const int UpdateIntervalInSecond = 3600;

        private const int MinPrice = 5;
        private const int DefaulPrice = 50;
        private const int MaxPrice = 1000;

        private const int QuantityPerChangePerPlayer = 200;

        public static void Init() {
            if (crop == null)
                crop = new Market(ResourceType.CROP, DefaulPrice, QuantityPerChangePerPlayer);

            if (iron == null)
                iron = new Market(ResourceType.IRON, DefaulPrice * 10, QuantityPerChangePerPlayer);

            if (wood == null)
                wood = new Market(ResourceType.WOOD, DefaulPrice, QuantityPerChangePerPlayer);
        }

        private static Market crop;
        private static Market iron;
        private static Market wood;

        public static Market Crop {
            get { return crop; }
            set { crop = value; }
        }

        public static Market Iron {
            get { return iron; }
            set { iron = value; }
        }

        public static Market Wood {
            get { return wood; }
            set { wood = value; }
        }

        private int incoming = 0;
        private int outgoing = 0;
        private int price;
        private int quantityPerChangePerPlayer;
        private DateTime time;
        private ResourceType resource;

        public Market(ResourceType resource, int defaultPrice, int quantityerChangePerPlayer) {
            price = defaultPrice;
            // parameter quantityerChangePerPlayer is not in used(so I can change the setting on the fly without changing database)
            this.quantityPerChangePerPlayer = QuantityPerChangePerPlayer;
            time = DateTime.UtcNow.AddSeconds(UpdateIntervalInSecond * Config.seconds_per_unit);
            this.resource = resource;
            Global.Scheduler.Put(this);
        }

        public void dbLoad(int outgoing, int incoming) {
            this.outgoing = outgoing;
            this.incoming = incoming;
        }

        public int Price {
            get { return price; }
        }

        public bool Buy(int quantity, int price) {
            lock (marketLock) {
                if (price != Price)
                    return false;
                outgoing += quantity;
                return true;
            }
        }

        public bool Sell(int quantity, int price) {
            lock (marketLock) {
                if (price != Price)
                    return false;
                incoming += quantity;
                return true;
            }
        }

        #region ISchedule Members

        public bool IsScheduled { get; set; }

        public DateTime Time {
            get { return time; }
        }

        public void Callback(object custom) {
            lock (marketLock) {
                using (DbTransaction transaction = Global.DbManager.GetThreadTransaction()) {
                    int flow = outgoing - incoming;
                    if (Global.Players.Count > 0) {
                        price += (flow / (quantityPerChangePerPlayer * Global.Players.Count));
                        if (price < MinPrice) price = MinPrice;
                        if (price > MaxPrice) price = MaxPrice;
                        outgoing = incoming = 0;
                    }
                    time = DateTime.UtcNow.AddSeconds(UpdateIntervalInSecond * Config.seconds_per_unit);
                    Global.DbManager.Save(this);
                    Global.Scheduler.Put(this);
                }
            }
        }

        #endregion

        public void Supply(ushort quantity) {
            lock (marketLock) {
                incoming += quantity;
            }
        }

        public void Consume(ushort quantity) {
            lock (marketLock) {
                outgoing += quantity;
            }
        }

        #region IPersistableObject Members

        public bool DbPersisted { get; set; }

        #endregion

        #region IPersistable Members

        public const string DB_TABLE = "market";

        public string DbTable {
            get { return DB_TABLE; }
        }

        public DbColumn[] DbPrimaryKey {
            get { return new DbColumn[] {new DbColumn("resource_type", (byte) resource, DbType.Byte)}; }
        }

        public DbDependency[] DbDependencies {
            get { return new DbDependency[] {}; }
        }

        public DbColumn[] DbColumns {
            get {
                return new DbColumn[] {
                                          new DbColumn("incoming", incoming, DbType.Int32),
                                          new DbColumn("outgoing", outgoing, DbType.Int32),
                                          new DbColumn("price", price, DbType.Int32),
                                          new DbColumn("quantity_per_change", quantityPerChangePerPlayer, DbType.Int32)
                                      };
            }
        }

        #endregion
    }
}