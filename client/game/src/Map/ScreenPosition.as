package src.Map {
    public class ScreenPosition {
        private var _x: Number;
        private var _y: Number;

        public function ScreenPosition(x: Number = 0, y: Number = 0) {
            this._x = x;
            this._y = y;
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

        public function toString(): String {
            return x + "," + y;
        }

        public function get x(): int {
            return _x;
        }

        public function get y(): int {
            return _y;
        }

        public function set x(value: int): void {
            _x = value;
        }

        public function set y(value: int): void  {
            _y = value;
        }

        public function setXAsNumber(value: Number): void {
            _x = value;
        }

        public function setYAsNumber(value: Number): void {
            _y = value;
        }

        public function getXAsNumber(): Number {
            return _x;
        }

        public function getYAsNumber(): Number {
            return _y;
        }
    }
}
