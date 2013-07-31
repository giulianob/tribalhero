package src.Map {
    import flash.geom.Point;

    public class ScreenPosition {
        public var x: int;
        public var y: int;

        public function ScreenPosition(x: int = 0, y:int = 0) {
            this.x = x;
            this.y = y;
        }

        public function equals(other: ScreenPosition): Boolean
        {
            return other.x == x && other.y == y;
        }

        public function toPosition(): Position
        {
            return TileLocator.getMapCoord(this);
        }

        public function copy(): ScreenPosition {
            return new ScreenPosition(x, y);
        }
    }
}
