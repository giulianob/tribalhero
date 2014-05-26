package src.UI.ViewModels {
    import com.codecatalyst.promise.Promise;

    import src.Global;
    import src.Objects.Store.StoreItem;
    import src.UI.ViewModel;

    public class StoreConfirmBuyVM extends ViewModel {
        public static const EVENT_CONFIRM_BUY_ITEM_SUCCESS: String = "EVENT_CONFIRM_BUY_ITEM_SUCCESS";
        public static const EVENT_CONFIRM_BUY_ITEM_FAILURE: String = "EVENT_CONFIRM_BUY_ITEM_FAILURE";

        private var _item: StoreItem;

        public function StoreConfirmBuyVM(item: StoreItem) {
            _item = item;
        }

        public function get item(): StoreItem {
            return _item;
        }

        public function buyItem(): Promise {
            return Global.mapComm.Store.purchaseItem(item.id).then(function(): void {
                dispatch(EVENT_CONFIRM_BUY_ITEM_SUCCESS, item);
            }).otherwise(function(): void {
                dispatch(EVENT_CONFIRM_BUY_ITEM_FAILURE, item);
            });
        }
    }
}