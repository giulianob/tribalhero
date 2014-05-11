package src.UI.ViewModels {
    import com.codecatalyst.promise.Promise;

    import src.Global;
    import src.Objects.Store.StoreItem;
    import src.Objects.Store.StoreItemTheme;
    import src.UI.Dialog.StoreViewThemeDetailsDialog;

    public class StoreDialogVM {
        public function viewItemDetails(item: StoreItem): void {
            if (item is StoreItemTheme) {
                new StoreViewThemeDetailsDialog(new StoreViewThemeDetailsVM(StoreItemTheme(item))).show();
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
