package src.Objects {
    import flash.geom.Point;

    import src.Constants;
    import src.Graphics.WallTileset;

    import starling.display.Image;

    public class WallObject extends SimpleObject {
        private var standardAsset: Image;
        private var theme: String;

        public var tileId: int;

        public function WallObject(objX: int, objY: int, tileId: int, theme: String) {
            super(objX, objY, 1);
            this.tileId = tileId;
            this.theme = theme;

            mapPriority = Constants.mapObjectPriority.wallPriority;

            this.standardAsset = WallTileset.getTile(theme, tileId);

            setSprite(standardAsset, new Point());
        }

        public function getTroopOverlappingAsset(): Image {
            return WallTileset.getTile(theme, tileId + 24);
        }

        override public function dim(): void {
            // Walls never dim
        }

        override public function setVisibilityPriority(isHighestPriority: Boolean, objectsInTile: Array): void {
            super.setVisibilityPriority(isHighestPriority, objectsInTile);

            visible = true;
        }
    }
}
