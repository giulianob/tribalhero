#region

using System;
using System.Data;
using Game.Data;
using Game.Database;
using Game.Logic;

#endregion

namespace Game.Module {
    public class Market : ISchedule, IPersistableObject {
        private const int DefaulPrice = 50;
        private const int QuantityPerChange = 4;
        private const int UpdateIntervalInSecond = 5;

        public static void Init() {
            if (crop == null)
                crop = new Market(ResourceType.Crop, DefaulPrice, QuantityPerChange);

            if (iron == null)
                iron = new Market(ResourceType.Iron, DefaulPrice, QuantityPerChange);

            if (wood == null)
                wood = new Market(ResourceType.Wood, DefaulPrice, QuantityPerChange);
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
        private int quantityPerChange;
        private DateTime time;
        private ResourceType resource;

        public Market(ResourceType resource, int defaultPrice, int quantityPerChange) {
            price = defaultPrice;
            this.quantityPerChange = quantityPerChange;
            time = DateTime.Now;
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
            lock (this) {
                if (price != Price)
                    return false;
                outgoing += quantity;
                return true;
            }
        }

        public bool Sell(int quantity, int price) {
            lock (this) {
                if (price != Price)
                    return false;
                incoming += quantity;
                return true;
            }
        }

        #region ISchedule Members

        public DateTime Time {
            get { return time.AddSeconds(UpdateIntervalInSecond); }
        }

        public void Callback(object custom) {
            lock (this) {
                using (DbTransaction transaction = Global.DbManager.GetThreadTransaction()) {
                    int flow = outgoing - incoming;
                    price += (flow/quantityPerChange);
                    outgoing = incoming = 0;
                    time = DateTime.Now.AddSeconds(UpdateIntervalInSecond);
                    Global.DbManager.Save(this);
                    Global.Scheduler.Put(this);
                }
            }
        }

        #endregion

        public void Supply(ushort quantity) {
            lock (this) {
                incoming += quantity;
            }
        }

        public void Consume(ushort quantity) {
            lock (this) {
                outgoing += quantity;
            }
        }

        #region IPersistableObject Members

        private bool dbPersisted = false;

        public bool DbPersisted {
            get { return dbPersisted; }
            set { dbPersisted = value; }
        }

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
                                          new DbColumn("outgoing", outgoing, DbType.Int32), new DbColumn("price", price, DbType.Int32),
                                          new DbColumn("quantity_per_change", quantityPerChange, DbType.Int32)
                                      };
            }
        }

        #endregion
    }
}