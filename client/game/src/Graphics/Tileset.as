package src.Graphics {
    import flash.geom.Rectangle;

    import src.Constants;
    import src.StarlingStage;

    import starling.display.Image;
    import starling.textures.Texture;

    public class Tileset {
        private static var tiles: Array = null;

        public static function getTile(tileId: int): Image {
            if (tiles === null) {
                tiles = new Array(Constants.tileSetTileW * Constants.tileSetTileH);

                var tileset: Texture = StarlingStage.assets.getTexture("TILESET");

                for (var row: int = 0; row < Constants.tileSetTileH; row++) {
                    for (var column: int = 0; column < Constants.tileSetTileW; column++) {
                        var initTileId: int = row * Constants.tileSetTileW + column;

                        var tileTexture: Texture = Texture.fromTexture(tileset, new Rectangle(column * Constants.tileW, row * Constants.tileH * 2, Constants.tileW, Constants.tileH * 2));
                        tiles[initTileId] = new Image(tileTexture);
                    }
                }
            }

            return tiles[tileId];
        }
    }
}
