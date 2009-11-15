using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace Game.Database {
    public class DbColumn {
        string column;
        public string Column {
            get { return column; }
        }

        object value;
        public object Value {
            get { return value; }
        }

        DbType type;
        public DbType Type {
            get { return type; }
        }

        int size = 0;
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

        public DbColumn(string column, object value, DbType type, int size):
          this(column, value, type) {
            this.size = size;
        }

    }
}
