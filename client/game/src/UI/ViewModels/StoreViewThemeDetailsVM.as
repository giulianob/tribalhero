package src.UI.ViewModels {
    import src.Assets;
    import src.Constants;
    import src.Objects.Factories.StructureFactory;
    import src.Objects.Prototypes.StructurePrototype;
    import src.Objects.Store.StoreItem;
    import src.Objects.Store.StoreItemTheme;
    import src.Objects.Store.StructureStoreAsset;
    import src.UI.Dialog.StoreBuyCoinsDialog;
    import src.UI.Dialog.StoreConfirmBuyDialog;

    public class StoreViewThemeDetailsVM {
        private var item: StoreItemTheme;

        public function StoreViewThemeDetailsVM(item: StoreItemTheme) {
            this.item = item;
        }

        public function get theme(): StoreItem {
            return item;
        }

        public function getThemeAssets(): Array {
            var themeItems: Array = [];

            for each (var structurePrototype: StructurePrototype in StructureFactory.getAllStructureTypes()) {
                if (Assets.doesSpriteExist(structurePrototype.getSpriteName(theme.id))) {
                    themeItems.push(new StructureStoreAsset(item, structurePrototype));
                }
            }

            themeItems.sortOn("title", Array.CASEINSENSITIVE);
            return themeItems;
        }

        public function buy(): void {
            if (Constants.session.coins < theme.cost) {
                var vm: StoreBuyCoinsVM = new StoreBuyCoinsVM(theme.cost);
                var buyCoinsDialog: StoreBuyCoinsDialog = new StoreBuyCoinsDialog(vm);

                vm.purchasedCoinsPromise.then(function(): void {
                    if (buyCoinsDialog.getFrame()) {
                        buyCoinsDialog.getFrame().dispose();
                    }

                    buy();
                });

                buyCoinsDialog.show();
            }
            else {
                showConfirmationDialog();
            }
        }

        private function showConfirmationDialog(): void {
            var confirmDialog: StoreConfirmBuyDialog = new StoreConfirmBuyDialog(new StoreConfirmBuyVM(item));
            confirmDialog.show();
        }
    }
}