namespace Game.Database {
    public class DbDependency {
        private string propertyName;

        public string Property {
            get { return propertyName; }
        }

        private bool autoSave = false;

        public bool AutoSave {
            get { return autoSave; }
        }

        private bool autoDelete = false;

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