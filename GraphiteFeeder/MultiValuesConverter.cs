using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Persistance;

namespace GraphiteFeeder
{
    class MultiValuesConverter : IKpvConverter 
    {
        private readonly string message;
        private readonly string sql;
        private readonly IDbManager dbManager;

        public MultiValuesConverter(string message, string sql, IDbManager dbManager)
        {
            this.message = message;
            this.sql = sql;
            this.dbManager = dbManager;
        }

        public IEnumerator<KeyValuePair<string, long>> GetEnumerator()
        {
            DbDataReader reader;
            try
            {
                reader = dbManager.ReaderQuery(sql);
            }
            catch
            {
                Console.Write("Failed to query [{0}]", sql);
                yield break;
            }

            if (reader.FieldCount < 1)
            {
                throw new Exception("SQL returned less than 1 value.");
            }
            while (reader.Read())
            {
                int valueFieldIndex = reader.FieldCount - 1;

                if (reader[valueFieldIndex].GetType() != typeof(Int64))
                {
                    throw new Exception("SQL returned a value that is not Int64.");
                }
                for (var i = 0; i < valueFieldIndex; ++i)
                {
                    var key = message.Replace(String.Format("({0})", i), reader[i].ToString());
                    yield return new KeyValuePair<string, long>(key, (long)reader[valueFieldIndex]);
                }
            }

        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
