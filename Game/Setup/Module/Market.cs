using System;
using System.Collections.Generic;
using System.Text;
using Game.Data;
using Game.Logic;
using Game.Database;

namespace Game.Module {
    public class Market : ISchedule, IPersistableObject {
        const int DefaulPrice = 500;
        const int QuantityPerChange = 4;
        const int UpdateIntervalInSecond = 5;

        public static void Init() {
            if (crop == null)
                crop = new Market(ResourceType.Crop, DefaulPrice, QuantityPerChange);
            
            if (iron == null)
                iron = new Market(ResourceType.Iron, DefaulPrice, QuantityPerChange);

            if (wood == null)
                wood = new Market(ResourceType.Wood, DefaulPrice, QuantityPerChange);
        }

        static Market crop;
        static Market iron;
        static Market wood;

        public static Market Crop { get { return crop; } set { crop = value; } }
        public static Market Iron { get { return iron; } set { iron = value; } }
        public static Market Wood { get { return wood; } set { wood = value; } }

        int incoming = 0;
        int outgoing = 0;
        int price;
        int quantityPerChange;
        DateTime time;
        ResourceType resource;

        public Market(ResourceType resource, int defaultPrice, int quantityPerChange) {
            this.price = defaultPrice;
            this.quantityPerChange = quantityPerChange;
            this.time = DateTime.Now;
            this.resource = resource;
            Scheduler.put(this);
        }

        public void dbLoad(int outgoing, int incoming) {
            this.outgoing = outgoing;
            this.incoming = incoming;
        }

        public int Price {
            get { return this.price; }
        }

        public bool Buy(int quantity, int price) {
            lock(this) {
                if (price != Price) return false;
                outgoing += quantity;
                return true;
            }
        }
        public bool Sell(int quantity, int price) {
            lock (this) {
                if (price != Price) return false;
                incoming += quantity;
                return true;
            }
        }

        #region ISchedule Members

        public DateTime Time {
            get { return time.AddSeconds(UpdateIntervalInSecond); }
        }

        public void callback(object custom) {
            lock (this) {
                int flow = outgoing - incoming;
                price += (flow / quantityPerChange);
                outgoing = incoming = 0;
                time = DateTime.Now.AddSeconds(UpdateIntervalInSecond);              
                Global.dbManager.Save(this);
                Scheduler.put(this);
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
        public const string DB_TABLE = "market";

        public string DbTable {
            get { return DB_TABLE; }
        }

        public DbColumn[] DbPrimaryKey {
            get {
                return new DbColumn[] { 
                    new DbColumn("resource_type", (byte)resource, System.Data.DbType.Byte)
                };
            }
        }

        public DbDependency[] DbDependencies {
            get { return new DbDependency[] { }; }
        }

        public DbColumn[] DbColumns {
            get {
                return new DbColumn[] {
                    new DbColumn("incoming", incoming, System.Data.DbType.Int32), 
                    new DbColumn("outgoing", outgoing, System.Data.DbType.Int32),
                    new DbColumn("price", price, System.Data.DbType.Int32),
                    new DbColumn("quantity_per_change", quantityPerChange, System.Data.DbType.Int32)
                };
            }
        }

        #endregion
    }
}
