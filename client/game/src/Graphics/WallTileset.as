package src.Graphics {

    import flash.geom.Rectangle;
    import flash.utils.Dictionary;

    import src.Constants;
    import src.Objects.Theme;
    import src.StarlingStage;

    import starling.display.Image;
    import starling.textures.Texture;

    public class WallTileset {
        private static var wallTiles: Dictionary = new Dictionary();

        public static function getTile(theme: String, tileId: int): Image {
            var wallTileName: String = getSpriteName(theme) + ":" + tileId;

            var wallTile: Texture = wallTiles[wallTileName];
            if (wallTile == null) {
                var tilesetsrcX:int = int(tileId % Constants.tileSetTileW) * Constants.tileW;
                var tilesetsrcY:int = int(tileId / Constants.tileSetTileW) * Constants.tileH * 2;
                var texture: Texture = StarlingStage.assets.getTexture(theme);
                if (texture == null) {
                    texture = StarlingStage.assets.getTexture(Theme.DEFAULT_THEME_ID);
                }

                wallTile = Texture.fromTexture(texture, new Rectangle(
                        tilesetsrcX, tilesetsrcY,
                        Constants.tileW, Constants.tileH * 2
                ));

                wallTiles[wallTileName] = wallTile;
            }

            var tile: Image = new Image(wallTile);
            tile.y = Constants.tileH * -1;
            return tile;
        }

        public static function getSpriteName(theme: String): String {
            return theme + "_WALL_TILESET";
        }
    }
}
