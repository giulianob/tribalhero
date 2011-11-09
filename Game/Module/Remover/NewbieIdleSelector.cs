using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game.Data;
using Game.Setup;
using Game.Util.Locking;
using Ninject;
using Persistance;

namespace Game.Module.Remover {
    public class NewbieIdleSelector : IPlayerSelector{

        #region IPlayerSelector Members

        public IEnumerable<uint> GetPlayerIds()
        {
            using (var reader =
                            Ioc.Kernel.Get<IDbManager>().ReaderQuery(
                                                                     string.Format(@"
                                                                                SELECT DISTINCT players.id player_id 
                                                                                FROM   players 
                                                                                        INNER JOIN cities 
                                                                                            ON cities.player_id = players.id 
                                                                                            AND cities.VALUE <= 5 
                                                                                WHERE  players.last_login < DATE_SUB(Utc_timestamp(), INTERVAL 3 DAY) 
                                                                                        AND players.online = 0 
                                                                                        AND (SELECT COUNT(*) 
                                                                                            FROM   cities c 
                                                                                            WHERE  c.player_id = players.id 
                                                                                            GROUP  BY c.player_id) = 1 
                                                                                ORDER  BY players.id ASC "
                                        ), new DbColumn[] { }))
            {
                while (reader.Read())
                    yield return (uint)reader["player_id"];
            }
        }

        #endregion
    }
}
