package src.UI {
    import flash.events.EventDispatcher;

    public class Flow {
        private var _eventDispatcher: EventDispatcher;

        public function Flow() {
            _eventDispatcher = new EventDispatcher();
        }

        public function on(event: String, listener: Function, ... params): void {
            var localListener: Function = listener;
            var localParams: Array = params;

            _eventDispatcher.addEventListener(event, function(e: FlowEvent): void {
                localListener.apply(null, localParams.concat(e.params));
            }, false, 0, false);
        }

        public function dispatch(eventType: String, ... params): void {
            _eventDispatcher.dispatchEvent(new FlowEvent(eventType, params));
        }
    }
}
