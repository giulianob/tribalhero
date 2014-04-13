package src.Objects {
    import flash.display.Bitmap;
    import flash.geom.Point;

    import src.Constants;

    public class WallObject extends SimpleObject {
        private var standardAsset: Bitmap;

        public var tileId: int;

        public function WallObject(objX: int, objY: int, tileId: int) {
            super(objX, objY, 1);
            this.tileId = tileId;

            mapPriority = Constants.mapObjectPriority.wallPriority;

            this.standardAsset = Constants.wallTileset.getTile(tileId);

            setSprite(standardAsset, new Point());
        }

        override public function dispose(): void {
            super.dispose();

            removeChildren();
            standardAsset.bitmapData.dispose();
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
