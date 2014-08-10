package src.Objects.Store {
    import flash.display.Bitmap;
    import flash.display.Sprite;

    import src.Constants;

    import src.Graphics.WallTileset;

    public class WallStoreAsset implements IStoreAsset {
        private var item: StoreItemTheme;

        public function WallStoreAsset(item: StoreItemTheme) {
            this.item = item;
        }

        public function title(): String {
            return t("STR_WALL_SET");
        }

        public function thumbnail(): Sprite {
            var sprite: Sprite = new Sprite();
            var wall: Bitmap = WallTileset.getFlashTile(item.themeId, 0);
            wall.y = Constants.tileH/4;
            sprite.addChild(wall);
            return sprite;
        }
    }
}
