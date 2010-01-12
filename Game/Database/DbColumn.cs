#region

using System.Data;

#endregion

namespace Game.Database {
    public class DbColumn {
        private string column;

        public string Column {
            get { return column; }
        }

        private object value;

        public object Value {
            get { return value; }
        }

        private DbType type;

        public DbType Type {
            get { return type; }
        }

        private int size = 0;

        public int Size {
            get { return size; }
        }

        public DbColumn(string column, DbType type) {
            this.column = column;
            this.type = type;
        }

        public DbColumn(string column, object value, DbType type) {
            this.column = column;
            this.value = value;
            this.type = type;
        }

        public DbColumn(string column, object value, DbType type, int size) : this(column, value, type) {
            this.size = size;
        }
    }
}