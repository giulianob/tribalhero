package src.Objects {
    import flash.display.Bitmap;

    import src.Assets;
    import src.Util.StringHelper;

    public class Theme {
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
