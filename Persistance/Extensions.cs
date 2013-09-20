using System.Collections.Generic;
using System.Data.Common;
using System.Dynamic;

namespace Persistance
{
    public static class Extensions
    {
        public static IEnumerable<IDictionary<string, object>> ReadAll(this DbDataReader reader)
        {
            var results = new List<IDictionary<string, object>>();

            while (reader.Read())
            {
                var expandoObject = new ExpandoObject() as IDictionary<string, object>;
                for (var i = 0; i < reader.FieldCount; i++)
                {
                    expandoObject.Add(reader.GetName(i), reader[i]);
                }

                results.Add(expandoObject);
            }

            reader.Close();

            return results;
        }
    }
}
