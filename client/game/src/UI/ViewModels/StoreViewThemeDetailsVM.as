package src.UI.ViewModels {
    import src.Assets;
    import src.Constants;
    import src.Objects.Factories.StructureFactory;
    import src.Objects.Prototypes.StructurePrototype;
    import src.Objects.Store.StructureStoreItem;
    import src.Objects.Theme;
    import src.UI.Dialog.StoreBuyCoinsDialog;
    import src.UI.Dialog.StoreConfirmBuyDialog;

    public class StoreViewThemeDetailsVM {
        private var _theme: Theme;

        public function StoreViewThemeDetailsVM(theme: Theme) {
            _theme = theme;
        }

        public function get theme(): Theme {
            return _theme;
        }

        public function getThemeItems(): Array {
            var themeItems: Array = [];

            for each (var structurePrototype: StructurePrototype in StructureFactory.getAllStructureTypes()) {
                if (Assets.doesSpriteExist(structurePrototype.getSpriteName(theme.id))) {
                    themeItems.push(new StructureStoreItem(theme, structurePrototype));
                }
            }

            themeItems.sortOn("title", Array.CASEINSENSITIVE);
            return themeItems;
        }

        public function buy(): void {
            if (Constants.coins < theme.cost) {
                var buyCoinsDialog: StoreBuyCoinsDialog = new StoreBuyCoinsDialog(new StoreBuyCoinsVM(theme.cost));
                buyCoinsDialog.show();
                buyCoinsDialog.purchaseThemePromise.then(buy);
            }
            else {
                showConfirmationDialog();
            }
        }

        private function showConfirmationDialog(): void {
            var confirmDialog: StoreConfirmBuyDialog = new StoreConfirmBuyDialog(new StoreConfirmBuyVM(theme));
            confirmDialog.show();
        }
    }
}