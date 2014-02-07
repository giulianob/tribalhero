using System.Collections.Generic;
using Game.Data;
using Game.Database;
using Persistance;

namespace Game.Module.Remover
{
    class ActivePlayerSelector : IPlayerSelector
    {
        private readonly int maxIdleDays;

        public ActivePlayerSelector(int maxIdleDays)
        {
            this.maxIdleDays = maxIdleDays;
        }

        public IEnumerable<uint> GetPlayerIds()
        {
            using (
                    var reader =
                            DbPersistance.Current.ReaderQuery(
                                                              string.Format(
                                                                            "SELECT players.id player_id FROM `{0}` WHERE TIMEDIFF(UTC_TIMESTAMP(), `last_login`) < '{1}:00:00.000000'",
                                                                            Player.DB_TABLE,
                                                                            maxIdleDays * 24),
                                                              new DbColumn[] {}))
            {
                while (reader.Read())
                {
                    yield return (uint)reader["player_id"];
                }
            }
        }
    }
}