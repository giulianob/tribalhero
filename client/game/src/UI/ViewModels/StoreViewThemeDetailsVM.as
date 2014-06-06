package src.UI.ViewModels {
    import src.Assets;
    import src.Constants;
    import src.Global;
    import src.Map.City;
    import src.Objects.Factories.StructureFactory;
    import src.Objects.Prototypes.StructurePrototype;
    import src.Objects.Store.StoreItemTheme;
    import src.Objects.Store.StructureStoreAsset;
    import src.UI.ViewModel;

    public class StoreViewThemeDetailsVM extends ViewModel {
        public static const EVENT_CONFIRM_PURCHASE_ITEM: String = "EVENT_CONFIRM_PURCHASE_ITEM";
        public static const EVENT_COIN_PURCHASE_NEEDED: String = "EVENT_COIN_PURCHASE_NEEDED";
        public static const EVENT_COMPLETED_APPLY_ALL_THEME: String = "EVENT_COMPLETED_APPLY_ALL_THEME";
        public static const EVENT_COMPLETED_APPLY_WALL_THEME: String = "EVENT_COMPLETED_APPLY_WALL_THEME";
        public static const EVENT_COMPLETED_SET_DEFAULT_THEME: String = "EVENT_COMPLETED_SET_DEFAULT_THEME";

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

        public function applyAllTheme(city: City): void {
            Global.mapComm.Store.applyThemeToAll(city.id, theme.themeId).then(function(): void {
               dispatch(EVENT_COMPLETED_APPLY_ALL_THEME);
            });
        }

        public function applyWallTheme(city: City): void {
            Global.mapComm.Store.setWallTheme(city.id, theme.themeId).then(function(): void {
               dispatch(EVENT_COMPLETED_APPLY_WALL_THEME);
            });
        }

        public function setDefaultTheme(city: City): void {
            Global.mapComm.Store.setDefaultTheme(city.id, theme.themeId).then(function(): void {
                dispatch(EVENT_COMPLETED_SET_DEFAULT_THEME);
            });
        }
    }
}