using System;
using System.Data;
using Game;
using Game.Data;
using Game.Data.Stats;
using Game.Database;
using Game.Setup;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Testing.Database {

    public class SimplePersistableObject : IPersistableObject {

        public int Id { get; set; }
        public int AnInt { get; set; }
        public float AFloat { get; set; }
        public string AString { get; set; }

        #region IPersistableObject Members 
        public bool DbPersisted { get; set; }
        #endregion

        #region IPersistable Members

        public string DbTable {
            get { return "simple_objects"; }
        }

        public DbColumn[] DbPrimaryKey {
            get {
                return new[]
                {
                    new DbColumn("id", Id, DbType.Int32)
                };
            }
        }

        public DbDependency[] DbDependencies {
            get { return new DbDependency[] { }; }
        }

        public DbColumn[] DbColumns {
            get {
                return new[]
                {
                    new DbColumn("a_float", AFloat, DbType.Double),
                    new DbColumn("an_int", AnInt, DbType.Int32),
                    new DbColumn("a_string", AString, DbType.String, 16)
                };
            }
        }

        #endregion
    }

    /// <summary>
    /// Summary description for DatabasePerformanceTest
    /// </summary>
    [TestClass]
    public class DatabasePerformanceTest {
        readonly IDbManager dbManager = new MySqlDbManager(Config.database_host, Config.database_username,
                                                        Config.database_password, Config.database_test);

        [TestInitialize]
        public void TestInitialize() {
            dbManager.Query("TRUNCATE `simple_objects`");
        }

        [TestCleanup]
        public void TestCleanup() {

        }

        /// <summary>
        /// Tests performance of saving basic object
        /// </summary>
        [TestMethod]
        public void TestSimpleObject() {
            DateTime now = DateTime.Now;
            for (int i = 0; i < 2000; i++) {
                SimplePersistableObject obj = new SimplePersistableObject() {
                    Id = i,
                    AnInt = i,
                    AFloat = i * 2.0f,
                    AString = i.ToString()
                };

                using (dbManager.GetThreadTransaction()) {
                    dbManager.Save(obj);
                }
            }

            double testTime = DateTime.Now.Subtract(now).TotalMilliseconds;
            Console.Out.WriteLine("TestSimpleObject: " + testTime + " ms");
        }
    }
}