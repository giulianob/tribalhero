package src.FeathersUI {
    import flash.events.Event;

    public class FlowEvent extends Event {
        private var _params: Array;

        public function FlowEvent(eventType: String, ... params) {
            super(eventType);
            _params = params[0];
        }

        public function get params(): Array {
            return _params;
        }
    }
}
