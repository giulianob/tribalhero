package src.Graphics {
    import flash.display.Bitmap;
    import flash.geom.Rectangle;

    import src.Constants;
    import src.FlashAssets;
    import src.Objects.Factories.SpriteFactory;

    import starling.display.Image;
    import starling.textures.Texture;

    public class Tileset {
        private static var tiles: Array = null;

        public static function getTile(tileId: int): Image {
            if (tiles === null) {
                tiles = new Array(Constants.tileSetTileW * Constants.tileSetTileH);

                var bmp: Bitmap = FlashAssets.getSharedInstance("TILESET");

                var tileset: Texture = Texture.fromBitmap(bmp, false, false, Constants.contentScaleFactorBaseline);

                // var tileset: Texture = SpriteFactory.getStarlingImage("TILESET").texture;

                for (var row: int = 0; row < Constants.tileSetTileH; row++) {
                    for (var column: int = 0; column < Constants.tileSetTileW; column++) {
                        var initTileId: int = row * Constants.tileSetTileW + column;

                        var tileTexture: Texture = Texture.fromTexture(tileset, new Rectangle(column * Constants.tileW, row * Constants.tileH, Constants.tileW, Constants.tileH));
                        tiles[initTileId] = new Image(tileTexture);
                    }
                }
            }

            return tiles[tileId];
        }
    }
}
