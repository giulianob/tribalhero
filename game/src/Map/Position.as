package src.Map {
    import flash.geom.Point;

    public class Position {
        public var x: int;
        public var y: int;

        public function Position(x: int, y: int) {
            this.x = x;
            this.y = y;
        }

        public function equals(other: Position): Boolean {
            return other.x == x && other.y == y;
        }

        public function top(): Position
        {
            return new Position(x, y - 2);
        }

        public function bottom(): Position
        {
            return new Position(x, y + 2);
        }

        public function left(): Position
        {
            return new Position(x - 1, y);
        }

        public function right(): Position
        {
            return new Position(x + 1, y);
        }

        public function topLeft(): Position
        {
            return y % 2 == 0 ? new Position(x - 1, y - 1) : new Position(x, y - 1);
        }

        public function topRight(): Position
        {
            return y % 2 == 0 ? new Position(x, y - 1) : new Position(x + 1, y - 1);
        }

        public function bottomLeft(): Position
        {
            return y % 2 == 0 ? new Position(x - 1, y + 1) : new Position(x, y + 1);
        }

        public function bottomRight(): Position
        {
            return y % 2 == 0 ? new Position(x, y + 1) : new Position(x + 1, y + 1);
        }

        public function toScreenPosition(): ScreenPosition {
            var pt: Point = TileLocator.getScreenCoord(x, y);
            return new ScreenPosition(pt.x, pt.y);
        }
    }
}
