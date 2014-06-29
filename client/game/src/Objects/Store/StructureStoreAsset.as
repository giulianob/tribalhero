package src.Objects.Store {
    import flash.display.Sprite;

    import src.Objects.Factories.SpriteFactory;

    import src.Objects.Factories.StructureFactory;
    import src.Objects.Prototypes.StructurePrototype;

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
            return SpriteFactory.getFlashSprite(StructureFactory.getSpriteName(item.themeId, structurePrototype.type, structurePrototype.level));
        }
    }
}
