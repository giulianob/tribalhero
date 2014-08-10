package src.Events {
    import starling.events.Event;

    public class NavigateEvent extends Event {
        private var _x: int;
        private var _y: int;

        public function NavigateEvent(type: String, x: int, y: int, bubbles: Boolean = false) {
            super(type, bubbles, data);
            _x = x;
            _y = y;
        }

        public function get x(): int {
            return _x;
        }

        public function get y(): int {
            return _y;
        }
    }
}
