package src.Graphics {

    import flash.display.Bitmap;
    import flash.display.BitmapData;
    import flash.geom.Point;
    import flash.geom.Rectangle;
    import flash.utils.Dictionary;

    import src.Constants;
    import src.FlashAssets;
    import src.Global;
    import src.Objects.Theme;
    import src.StarlingStage;

    import starling.display.Image;
    import starling.display.Sprite;
    import starling.textures.Texture;

    public class WallTileset {
        private static var wallTiles: Dictionary = new Dictionary();

        public static function getTile(theme: String, tileId: int): Image {
            var wallTileName: String = getSpriteName(theme) + ":" + tileId;

            var wallTile: Texture = wallTiles[wallTileName];
            if (wallTile == null) {
                var tilesetsrcX:int = int(tileId % Constants.tileSetTileW) * Constants.tileW;
                var tilesetsrcY:int = int(tileId / Constants.tileSetTileW) * Constants.tileH * 2;
                var texture: Texture = Global.starlingStage.assets.getTexture(getSpriteName(theme));
                if (texture == null) {
                    texture = Global.starlingStage.assets.getTexture(getSpriteName(Theme.DEFAULT_THEME_ID));
                }

                wallTile = Texture.fromTexture(texture, new Rectangle(
                        tilesetsrcX, tilesetsrcY,
                        Constants.tileW, Constants.tileH * 2
                ));

                wallTiles[wallTileName] = wallTile;
            }

            var tile: Image = new Image(wallTile);
            tile.pivotY = Constants.tileH;

            return tile;
        }

        public static function getFlashTile(theme: String, tileId: int): Bitmap {
            var wallTileset: Bitmap = FlashAssets.getSharedInstance(getSpriteName(theme));

            var tilesetsrcX:int = int(tileId % Constants.tileSetTileW) * Constants.tileW;
            var tilesetsrcY:int = int(tileId / Constants.tileSetTileW) * Constants.tileH * 2;

            var tile: Bitmap = new Bitmap(new BitmapData(Constants.tileW, Constants.tileH * 2, true, 0));
            tile.y = Constants.tileH * -1;

            tile.bitmapData.copyPixels(
                    wallTileset.bitmapData,
                    new Rectangle(tilesetsrcX, tilesetsrcY, Constants.tileW, Constants.tileH * 2),
                    new Point(0, 0),
                    null,
                    null,
                    true);

            return tile;
        }

        public static function getSpriteName(theme: String): String {
            return theme + "_WALL";
        }
    }
}
