package src.Objects.Store {
    import flash.display.Sprite;

    import src.Objects.Factories.StrongholdFactory;

    public class StrongholdStoreAsset implements IStoreAsset {
        private var item: StoreItemTheme;

        public function StrongholdStoreAsset(item: StoreItemTheme) {
            this.item = item;
        }

        public function title(): String {
            return t("STR_STRONGHOLD");
        }

        public function thumbnail(): Sprite {
            return Sprite(StrongholdFactory.getSprite(item.themeId));
        }
    }
}
