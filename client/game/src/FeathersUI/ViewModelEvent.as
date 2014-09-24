package src.FeathersUI {
    import flash.events.Event;

    public class ViewModelEvent extends Event {
        private var _params: Array;

        public function ViewModelEvent(eventType: String, ... params) {
            super(eventType);
            _params = params[0];
        }

        public function get params(): Array {
            return _params;
        }
    }
}
