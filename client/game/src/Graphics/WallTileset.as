package src.Graphics {
    import flash.display.Bitmap;
    import flash.display.BitmapData;
    import flash.display.DisplayObject;
    import flash.geom.Point;
    import flash.geom.Rectangle;

    import src.Assets;

    import src.Constants;
    import src.Objects.SimpleObject;
    import src.Objects.Theme;

    public class WallTileset {


        public static function getTile(theme: String, tileId: int): Bitmap {
            var wallTileset: Bitmap;

            if (hasWall(theme)) {
                wallTileset = Assets.getSharedInstance(getSpriteName(theme));
            }
            else {
                wallTileset = Assets.getSharedInstance(getSpriteName(Theme.DEFAULT_THEME_ID));
            }

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
            return theme + "_WALL_TILESET";
        }

        public static function hasWall(theme: String): Boolean {
            return Assets.doesSpriteExist(getSpriteName(theme));
        }

        public static function useDefaultWall(theme: String): Boolean {
            return theme == Theme.DEFAULT_THEME_ID || !hasWall(theme);
        }
    }
}
