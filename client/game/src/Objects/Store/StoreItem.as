package src.Objects.Store {
    import flash.display.Bitmap;

    public class StoreItem {
        public static const STORE_ITEM_THEME: int = 0;
        public static const STORE_ITEM_ACHIEVEMENT: int = 1;

        public var id: String;
        public var cost: int;
        public var created: Date;

        public function StoreItem(id: String, cost: int, created: Date) {
            this.id = id;
            this.cost = cost;
            this.created = created;
        }

        public function get itemType(): int {
            throw new Error("Not implemented in child class");
        }

        public function hasPurchased(): Boolean {
            throw new Error("Not implemented in child class");
        }

        public function get localizedName(): String {
            throw new Error("Not implemented in child class");
        }

        public function get localizedDescription(): String {
            throw new Error("Not implemented in child class");
        }

        public function thumbnail(): Bitmap {
            throw new Error("Not implemented in child class");
        }

        public function get localizedPurchasedMessage(): String {
            throw new Error("Not implemented in child class");
        }
    }
}
