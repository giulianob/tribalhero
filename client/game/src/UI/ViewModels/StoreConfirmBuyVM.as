package src.UI.ViewModels {
    import com.codecatalyst.promise.Promise;

    import src.Global;
    import src.Objects.Store.StoreItem;
    import src.UI.ViewModel;

    public class StoreConfirmBuyVM extends ViewModel {
        public static const EVENT_CONFIRM_BUY_ITEM: String = "EVENT_CONFIRM_BUY_ITEM";

        private var _item: StoreItem;

        public function StoreConfirmBuyVM(item: StoreItem) {
            _item = item;
        }

        public function get item(): StoreItem {
            return _item;
        }

        public function buyItem(): Promise {
            return Global.mapComm.Store.purchaseItem(item.id).always(function(): void {
                dispatch(EVENT_CONFIRM_BUY_ITEM, item);
            });
        }
    }
}