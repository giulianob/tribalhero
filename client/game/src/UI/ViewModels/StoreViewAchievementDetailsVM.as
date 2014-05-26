package src.UI.ViewModels {
    import src.Assets;
    import src.Constants;
    import src.Global;
    import src.Map.City;
    import src.Objects.Factories.StructureFactory;
    import src.Objects.Prototypes.StructurePrototype;
    import src.Objects.Store.StoreItemAchievement;
    import src.Objects.Store.StoreItemTheme;
    import src.Objects.Store.StructureStoreAsset;
    import src.UI.ViewModel;

    public class StoreViewAchievementDetailsVM extends ViewModel {
        public static const EVENT_CONFIRM_PURCHASE_ITEM: String = "EVENT_CONFIRM_PURCHASE_ITEM";
        public static const EVENT_COIN_PURCHASE_NEEDED: String = "EVENT_COIN_PURCHASE_NEEDED";

        private var item: StoreItemAchievement;

        public function StoreViewAchievementDetailsVM(item: StoreItemAchievement) {
            this.item = item;
        }

        public function get achievement(): StoreItemAchievement {
            return item;
        }

        public function buy(): void {
            if (Constants.session.coins < achievement.cost) {
                dispatch(EVENT_COIN_PURCHASE_NEEDED, achievement);
            }
            else {
                dispatch(EVENT_CONFIRM_PURCHASE_ITEM, achievement);
            }
        }
    }
}