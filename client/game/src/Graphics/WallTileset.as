package src.Graphics {
    import flash.display.Bitmap;
    import flash.display.BitmapData;
    import flash.display.DisplayObject;
    import flash.geom.Point;
    import flash.geom.Rectangle;

    import src.Constants;
    import src.Objects.SimpleObject;

    [Embed(source = "../../../../graphics/WallTilemap.png")]
    public class WallTileset extends Bitmap {
        public function WallTileset() { }

        public function getTile(tileId: int): Bitmap {
            var tilesetsrcX:int = int(tileId % Constants.tileSetTileW) * Constants.tileW;
            var tilesetsrcY:int = int(tileId / Constants.tileSetTileW) * Constants.tileH * 2;

            var tile: Bitmap = new Bitmap(new BitmapData(Constants.tileW, Constants.tileH, true, 0));

            tile.bitmapData.copyPixels(
                    Constants.wallTileset.bitmapData,
                    new Rectangle(tilesetsrcX, tilesetsrcY, Constants.tileW, Constants.tileH * 2),
                    new Point(0, -Constants.tileH),
                    null,
                    null,
                    true);

            return tile;
        }
    }
}
