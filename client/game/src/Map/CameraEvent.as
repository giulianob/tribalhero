package src.Map {
    import flash.events.Event;

    public class CameraEvent extends Event {
        public var programmatic: Boolean;

        public function CameraEvent(type: String, programmatic: Boolean, bubbles: Boolean = false, cancelable: Boolean = false) {
            super(type, bubbles, cancelable);
            this.programmatic = programmatic;
        }
    }
}
