using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game.Data;
using Game.Database;
using Game.Setup;
using Ninject;
using Persistance;

namespace Game.Module.Remover {
    internal class ActivePlayerSelector: IPlayerSelector {
        int maxIdleDays;

        public ActivePlayerSelector(int maxIdleDays)
        {
            this.maxIdleDays = maxIdleDays;
        }

        public IEnumerable<uint> GetPlayerIds()
        {
            using (var reader =
                            DbPersistance.Current.ReaderQuery(
                                                                    string.Format(
                                                                       "SELECT players.id player_id FROM `{0}` WHERE TIMEDIFF(UTC_TIMESTAMP(), `last_login`) < '{1}:00:00.000000'",
                                                                       Player.DB_TABLE,
                                                                       maxIdleDays * 24
                                        ), new DbColumn[] { })) {
                while (reader.Read())
                    yield return (uint)reader["player_id"];
            }
        }
    }
}
