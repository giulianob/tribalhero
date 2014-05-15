package src.UI.ViewModels {
    import com.codecatalyst.promise.Promise;

    import src.Global;
    import src.Objects.Store.StoreItem;

    public class StoreConfirmBuyVM {
        private var _item: StoreItem;

        public function StoreConfirmBuyVM(item: StoreItem) {
            _item = item;
        }

        public function get item(): StoreItem {
            return _item;
        }

        public function buyItem(): Promise {
            return Global.mapComm.Store.purchaseItem(item.id).then(function(): void {
                item.markAsPurchased();
            });
        }
    }
}