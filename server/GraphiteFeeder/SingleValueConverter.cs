using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using Persistance;

namespace GraphiteFeeder
{
    public class SingleValueConverter : IKpvConverter
    {
        private readonly string key;
        private readonly string sql;
        private readonly IDbManager dbManager;

        public SingleValueConverter(string key, string sql, IDbManager dbManager)
        {
            this.key = key;
            this.sql = sql;
            this.dbManager = dbManager;
        }

        public IEnumerator<KeyValuePair<string, long>> GetEnumerator()
        {
            DbDataReader reader =
                    dbManager.ReaderQuery(sql);
            if (reader.FieldCount != 1)
            {
                throw new Exception("SQL returned more than 1 value.");
            }

            if (!reader.Read())
                yield break;

            if (reader[0].GetType() != typeof(Int64))
            {
                throw new Exception("SQL returned a value that is not Int64.");
            }
                
            var kvp = new KeyValuePair<string, long>(key,(long)reader[0]);
            yield return kvp;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
