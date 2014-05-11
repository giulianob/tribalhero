package src.Objects.Store {
    import flash.display.Sprite;

    import src.Objects.Factories.StructureFactory;
    import src.Objects.Prototypes.StructurePrototype;
    import src.Objects.Theme;
    import src.UI.Dialog.StoreViewThemeDetailsDialog;

    public class StructureStoreAsset implements IStoreAsset {
        private var structurePrototype: StructurePrototype;
        private var item: StoreItemTheme;

        public function StructureStoreAsset(item: StoreItemTheme, structurePrototype: StructurePrototype) {
            this.item = item;
            this.structurePrototype = structurePrototype;
        }

        public function title(): String {
            return structurePrototype.getName();
        }

        public function thumbnail(): Sprite {
            return Sprite(StructureFactory.getSprite(item.themeId, structurePrototype.type, structurePrototype.level));
        }
    }
}
