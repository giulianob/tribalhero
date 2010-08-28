namespace Game.Database {
    public class DbDependency {
        public string Property { get; private set; }

        public bool AutoSave { get; private set; }

        public bool AutoDelete { get; private set; }

        public DbDependency(string propertyName, bool autoSave, bool autoDelete) {
            Property = propertyName;
            AutoSave = autoSave;
            AutoDelete = autoDelete;
        }
    }
}