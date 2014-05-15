package src.UI.ViewModels {
    import src.Assets;
    import src.Constants;
    import src.Objects.Factories.StructureFactory;
    import src.Objects.Prototypes.StructurePrototype;
    import src.Objects.Store.StoreItemTheme;
    import src.Objects.Store.StructureStoreAsset;
    import src.UI.Dialog.StoreBuyCoinsDialog;
    import src.UI.Dialog.StoreConfirmBuyDialog;
    import src.UI.ViewModel;

    public class StoreViewThemeDetailsVM extends ViewModel {
        public static const EVENT_CONFIRM_PURCHASE_ITEM: String = "EVENT_CONFIRM_PURCHASE_ITEM";
        public static const EVENT_COIN_PURCHASE_NEEDED: String = "EVENT_COIN_PURCHASE_NEEDED";

        private var item: StoreItemTheme;


        public function StoreViewThemeDetailsVM(item: StoreItemTheme) {
            this.item = item;
        }

        public function get theme(): StoreItemTheme {
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
                dispatch(EVENT_COIN_PURCHASE_NEEDED, theme);
            }
            else {
                dispatch(EVENT_CONFIRM_PURCHASE_ITEM, theme);
            }
        }
    }
}