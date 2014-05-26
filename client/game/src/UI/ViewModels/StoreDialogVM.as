package src.UI.ViewModels {
    import com.codecatalyst.promise.Promise;

    import src.Global;
    import src.Objects.Store.StoreItem;
    import src.Objects.Store.StoreItemAchievement;
    import src.Objects.Store.StoreItemTheme;
    import src.UI.ViewModel;

    public class StoreDialogVM extends ViewModel {
        public static const EVENT_VIEW_THEME: String = "EVENT_VIEW_THEME";
        public static const EVENT_VIEW_ACHIEVEMENT: String = "EVENT_VIEW_ACHIEVEMENT";
        public static const EVENT_WANT_TO_PURCHASE_COINS: String = "EVENT_WANT_TO_PURCHASE_COINS";

        public function viewItemDetails(item: StoreItem): void {
            if (item is StoreItemTheme) {
                dispatch(EVENT_VIEW_THEME, item);
            }
            else if (item is StoreItemAchievement) {
                dispatch(EVENT_VIEW_ACHIEVEMENT, item);
            }
            else {
                throw new Error("Unknown item type");
            }
        }

        public function buyCoins(): void {
            dispatch(EVENT_WANT_TO_PURCHASE_COINS);
        }

        public function loadItems(): Promise {
            return Global.mapComm.Store.getItems();
        }
    }
}
