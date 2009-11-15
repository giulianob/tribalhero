using System;
using System.Collections.Generic;
using System.Text;

namespace Game.Database {
    public class DbDependency {
        string propertyName;
        public string Property {
            get { return propertyName; }
        }

        bool autoSave = false;
        public bool AutoSave {
            get { return autoSave; }
        }

        bool autoDelete = false;
        public bool AutoDelete {
            get { return autoSave; }
        }

        public DbDependency(string propertyName, bool autoSave, bool autoDelete) {
            this.propertyName = propertyName;
            this.autoSave = autoSave;
            this.autoDelete = autoDelete;
        }
    }
}
