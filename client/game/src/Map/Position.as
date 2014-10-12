package src.Map {
    import mx.utils.StringUtil;

    import src.Constants;

    import src.Util.StringHelper;

    public class Position {
        public var x: int;
        public var y: int;

        public function Position(x: int = 0, y: int = 0) {
            this.x = x;
            this.y = y;
        }

        public function equals(other: Position): Boolean {
            return other.x == x && other.y == y;
        }

        public static function sort(a:Position, b:Position):int {
            var aX:Number = a.x;
            var bX:Number = b.x;

            var aY:Number = a.y;
            var bY:Number = b.y;

            if (aX > bX)
                return 1;
            else if (aX < bX)
                return -1;
            else if (aY > bY)
                return 1;
            else if (aY < bY)
                return -1;
            else
                return 0;
        }

        public static function compare(a: Position, value: *):int
        {
            if (value is Position) {
                return sort(a, value as Position);
            }

            var xDelta: int = a.x - value[0];
            var yDelta: int = a.y - value[1];

            if (xDelta != 0)
                return xDelta;
            else if (yDelta != 0)
                return yDelta;
            else
                return 0;
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
            return TileLocator.getScreenCoord(this);
        }

        public function toTileCenteredScreenPosition(): ScreenPosition {
            var screenPos: ScreenPosition = TileLocator.getScreenCoord(this);
            screenPos.x += Constants.tileW/2;
            screenPos.y += Constants.tileH/2;

            return screenPos;
        }

        public function toString(): String {
            return StringUtil.substitute("({0},{1})", x, y);
        }
    }
}
