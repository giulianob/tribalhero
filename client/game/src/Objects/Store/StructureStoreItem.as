package src.Objects.Store {
    import flash.display.Sprite;

    import src.Objects.Factories.StructureFactory;
    import src.Objects.Prototypes.StructurePrototype;
    import src.Objects.Theme;
    import src.UI.Dialog.StoreViewThemeDetails;

    public class StructureStoreItem implements IStoreItem {
        private var structurePrototype: StructurePrototype;
        private var theme: Theme;

        public function StructureStoreItem(theme: Theme, structurePrototype: StructurePrototype) {
            this.theme = theme;
            this.structurePrototype = structurePrototype;
        }

        public function title(): String {
            return structurePrototype.getName();
        }

        public function thumbnail(): Sprite {
            return Sprite(StructureFactory.getSprite(theme.id, structurePrototype.type, structurePrototype.level));
        }
    }
}
