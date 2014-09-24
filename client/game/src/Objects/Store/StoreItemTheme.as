package src.Objects.Store {
    import flash.display.Bitmap;

    import src.FlashAssets;
    import src.Constants;
    import src.Util.StringHelper;

    public class StoreItemTheme extends StoreItem {
        public function StoreItemTheme(id: String, cost: int, created: Date) {
            super(id, cost, created);
        }

        public function get themeId(): String {
            return id;
        }

        override public function get itemType(): int {
            return STORE_ITEM_THEME;
        }

        override public function get localizedPurchasedMessage(): String {
            return StringHelper.localize("THEME_PURCHASED_MESSAGE");
        }

        override public function hasPurchased(): Boolean {
            return Constants.session.themesPurchased.indexOf(themeId) > -1;
        }

        override public function get localizedName(): String {
            return StringHelper.localize(id + "_THEME_NAME");
        }

        override public function get localizedDescription(): String {
            return StringHelper.localize(id + "_THEME_DESCRIPTION");
        }

        override public function thumbnail(): Bitmap {
            return FlashAssets.getInstance(id + "_THEME_THUMBNAIL")
        }
    }
}
