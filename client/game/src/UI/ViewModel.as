package src.UI {
    import flash.events.EventDispatcher;

    public class ViewModel {
        private var _eventDispatcher: EventDispatcher;

        public function ViewModel() {
            _eventDispatcher = new EventDispatcher();
        }

        public function on(event: String, listener: Function, ... params): void {
            var localListener: Function = listener;
            var localParams: Array = params;

            _eventDispatcher.addEventListener(event, function(e: ViewModelEvent): void {
                localListener.apply(null, localParams.concat(e.params));
            }, false, 0, true);
        }

        public function dispatch(eventType: String, ... params): void {
            _eventDispatcher.dispatchEvent(new ViewModelEvent(eventType, params));
        }
    }
}
