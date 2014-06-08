package src.Objects {
    import src.Util.StringHelper;

    public class Theme {
        public static const DEFAULT_THEME_ID: String = "DEFAULT";

        public var id: String;
        public var cost: int;
        public var created: Date;

        public function Theme(id: String, cost: int, created: Date) {
            this.id = id;
            this.cost = cost;
            this.created = created;
        }

        public function get localizedName(): String {
            return StringHelper.localize(id + "_THEME_NAME");
        }

        public function get localizedDescription(): String {
            return StringHelper.localize(id + "_THEME_DESCRIPTION");
        }
    }
}
