package src.Objects.Store {
    import flash.display.Bitmap;

    import src.Assets;
    import src.Constants;
    import src.Util.StringHelper;

    public class StoreItemAchievement extends StoreItem {
        public function StoreItemAchievement(id: String, cost: int, created: Date) {
            super(id, cost, created);
        }

        override public function hasPurchased(): Boolean {
            return false;
        }

        override public function get itemType(): int {
            return STORE_ITEM_ACHIEVEMENT;
        }

        override public function get localizedPurchasedMessage(): String {
            return StringHelper.localize("ACHIEVEMENT_PURCHASED_MESSAGE");
        }

        override public function get localizedName(): String {
            return StringHelper.localize(id + "_NAME");
        }

        override public function get localizedDescription(): String {
            return StringHelper.localize(id + "_DESCRIPTION");
        }

        override public function thumbnail(): Bitmap {
            return Assets.getInstance(id + "_THUMBNAIL")
        }
    }
}
