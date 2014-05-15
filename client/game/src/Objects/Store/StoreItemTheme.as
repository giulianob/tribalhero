package src.Objects.Store {
    import flash.display.Bitmap;

    import src.Assets;
    import src.Constants;
    import src.Util.StringHelper;

    public class StoreItemTheme extends StoreItem {
        public function StoreItemTheme(id: String, cost: int, created: Date) {
            super(id, cost, created);
        }

        public function get themeId(): String {
            return id;
        }

        override public function get localizedName(): String {
            return StringHelper.localize(id + "_THEME_NAME");
        }

        override public function get localizedDescription(): String {
            return StringHelper.localize(id + "_THEME_DESCRIPTION");
        }

        override public function thumbnail(): Bitmap {
            return Assets.getInstance(id + "_THEME_THUMBNAIL")
        }

        override public function markAsPurchased(): void {
            Constants.session.themesPurchased.push(themeId);
        }
    }
}
