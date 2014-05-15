package src.UI.ViewModels {
    import com.codecatalyst.promise.Promise;

    import src.Global;
    import src.Objects.Store.StoreItem;
    import src.Objects.Store.StoreItemTheme;
    import src.UI.ViewModel;

    public class StoreDialogVM extends ViewModel {
        public static const EVENT_VIEW_THEME: String = "EVENT_VIEW_THEME";

        public function viewItemDetails(item: StoreItem): void {
            if (item is StoreItemTheme) {
                dispatch(EVENT_VIEW_THEME, item);
            }
            else {
                throw new Error("Unknown item type");
            }
        }

        public function loadItems(): Promise {
            return Global.mapComm.Store.getItems();
        }
    }
}
