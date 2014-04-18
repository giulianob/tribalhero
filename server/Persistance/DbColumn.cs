#region

using System.Data;

#endregion

namespace Persistance
{
    public class DbColumn
    {
        public DbColumn(string column, DbType type)
        {
            Size = 0;
            Column = column;
            Type = type;
        }

        public DbColumn(string column, object value, DbType type)
        {
            Size = 0;
            Column = column;
            Value = value;
            Type = type;
        }

        public DbColumn(string column, object value, DbType type, int size)
                : this(column, value, type)
        {
            Size = size;
        }

        public string Column { get; private set; }

        public object Value { get; private set; }

        public DbType Type { get; private set; }

        public int Size { get; private set; }
    }
}