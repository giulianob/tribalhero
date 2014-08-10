package src.UI.ViewModels {
    import System.Linq.Enumerable;

    import src.FlashAssets;
    import src.Constants;
    import src.Global;
    import src.Graphics.WallTileset;
    import src.Map.City;
    import src.Objects.Factories.ObjectFactory;
    import src.Objects.Factories.StrongholdFactory;
    import src.Objects.Factories.StructureFactory;
    import src.Objects.Factories.TroopFactory;
    import src.Objects.Prototypes.ObjectTypePrototype;
    import src.Objects.Prototypes.StructurePrototype;
    import src.Objects.Store.IStoreAsset;
    import src.Objects.Store.StoreItemTheme;
    import src.Objects.Store.StrongholdStoreAsset;
    import src.Objects.Store.StructureStoreAsset;
    import src.Objects.Store.TroopStoreAsset;
    import src.Objects.Store.WallStoreAsset;
    import src.UI.ViewModel;

    public class StoreViewThemeDetailsVM extends ViewModel {
        public static const EVENT_CONFIRM_PURCHASE_ITEM: String = "EVENT_CONFIRM_PURCHASE_ITEM";
        public static const EVENT_COIN_PURCHASE_NEEDED: String = "EVENT_COIN_PURCHASE_NEEDED";
        public static const EVENT_COMPLETED_APPLY_ALL_THEME: String = "EVENT_COMPLETED_APPLY_ALL_THEME";
        public static const EVENT_COMPLETED_APPLY_WALL_THEME: String = "EVENT_COMPLETED_APPLY_WALL_THEME";
        public static const EVENT_COMPLETED_APPLY_ROAD_THEME: String = "EVENT_COMPLETED_APPLY_ROAD_THEME";
        public static const EVENT_COMPLETED_SET_DEFAULT_THEME: String = "EVENT_COMPLETED_SET_DEFAULT_THEME";
        public static const EVENT_COMPLETED_SET_DEFAULT_TROOP_THEME: String = "EVENT_COMPLETED_SET_DEFAULT_TROOP_THEME";

        private var item: StoreItemTheme;

        public function StoreViewThemeDetailsVM(item: StoreItemTheme) {
            this.item = item;
        }

        public function get theme(): StoreItemTheme {
            return item;
        }

        public function isStrongholdIncluded(): Boolean {
            return FlashAssets.doesSpriteExist(StrongholdFactory.getSpriteName(theme.id));
        }

        public function isWallIncluded(): Boolean {
            return FlashAssets.doesSpriteExist(WallTileset.getSpriteName(theme.id));
        }

        public function isRoadIncluded(): Boolean {
            return Constants.roadThemes.indexOf(theme.id) >= 0;
        }

        public function isTroopIncluded(): Boolean {
            return FlashAssets.doesSpriteExist(TroopFactory.getSpriteName(theme.id));
        }

        public function areStructuresIncluded(): Boolean {
            return FlashAssets.doesSpriteExist(StructureFactory.getSpriteName(theme.id, ObjectFactory.getFirstType("MainBuilding"), 1));
        }

        public function getThemeAssets(): Array {
            var themeItems: Array = [];

            if (isStrongholdIncluded()) {
                themeItems.push(new StrongholdStoreAsset(item));
            }

            if (isWallIncluded()) {
                themeItems.push(new WallStoreAsset(item));
            }

            if (isTroopIncluded()) {
                themeItems.push(new TroopStoreAsset(item));
            }

            for each (var structurePrototype: StructurePrototype in StructureFactory.getAllStructureTypes()) {
                if (FlashAssets.doesSpriteExist(structurePrototype.getSpriteName(theme.id))) {
                    themeItems.push(new StructureStoreAsset(item, structurePrototype));
                }
            }

            return Enumerable.from(themeItems).orderBy(function(themeItem: IStoreAsset): String {
                return themeItem.title();
            }).toArray();
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

        public function applyRoadTheme(city: City): void {
            Global.mapComm.Store.setRoadTheme(city.id, theme.themeId).then(function(): void {
               dispatch(EVENT_COMPLETED_APPLY_ROAD_THEME);
            });
        }

        public function setDefaultTheme(city: City): void {
            Global.mapComm.Store.setDefaultTheme(city.id, theme.themeId).then(function(): void {
                dispatch(EVENT_COMPLETED_SET_DEFAULT_THEME);
            });
        }

        public function setDefaultTroopTheme(city: *): void {
            Global.mapComm.Store.setDefaultTroopTheme(city.id, theme.themeId).then(function(): void {
                dispatch(EVENT_COMPLETED_SET_DEFAULT_TROOP_THEME);
            });
        }
    }
}