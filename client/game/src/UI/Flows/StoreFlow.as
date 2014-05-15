package src.UI.Flows {
    import src.Objects.Store.StoreItem;
    import src.Objects.Store.StoreItemTheme;
    import src.UI.Dialog.StoreBuyCoinsDialog;
    import src.UI.Dialog.StoreConfirmBuyDialog;
    import src.UI.Dialog.StoreDialog;
    import src.UI.Dialog.StoreViewThemeDetailsDialog;
    import src.UI.ViewModels.StoreBuyCoinsVM;
    import src.UI.ViewModels.StoreConfirmBuyVM;
    import src.UI.ViewModels.StoreDialogVM;
    import src.UI.ViewModels.StoreViewThemeDetailsVM;

    public class StoreFlow {
        public function showStore(): void {
            var storeVm: StoreDialogVM = new StoreDialogVM();
            storeVm.on(StoreDialogVM.EVENT_VIEW_THEME, viewThemeDetails);

            new StoreDialog(storeVm).show();
        }

        private function viewThemeDetails(item: StoreItemTheme): void {
            var vm: StoreViewThemeDetailsVM = new StoreViewThemeDetailsVM(item);
            var viewItemDetailsDialog: StoreViewThemeDetailsDialog = new StoreViewThemeDetailsDialog(vm);

            vm.on(StoreViewThemeDetailsVM.EVENT_COIN_PURCHASE_NEEDED, function(item: StoreItem): void {
                viewItemDetailsDialog.getFrame().dispose();
                needToPurchaseCoins(item);
            });

            vm.on(StoreViewThemeDetailsVM.EVENT_CONFIRM_PURCHASE_ITEM, function(item: StoreItem): void {
                viewItemDetailsDialog.getFrame().dispose();
                confirmPurchaseItem(item);
            });

            viewItemDetailsDialog.show();
        }

        private function confirmPurchaseItem(item: StoreItem): void {
            new StoreConfirmBuyDialog(new StoreConfirmBuyVM(item)).show();
        }

        private function needToPurchaseCoins(item: StoreItem): void {
            var vm: StoreBuyCoinsVM = new StoreBuyCoinsVM(item.cost);
            var buyCoinsDialog: StoreBuyCoinsDialog = new StoreBuyCoinsDialog(vm);
            vm.on(StoreBuyCoinsVM.EVENT_ITEM_COST_COINS_PURCHASED, function(item: StoreItem): void {
                buyCoinsDialog.getFrame().dispose();
                confirmPurchaseItem(item);
            }, item);

            buyCoinsDialog.show();
        }
    }
}
