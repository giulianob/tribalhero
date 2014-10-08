package src.FeathersUI {
    import flash.events.EventDispatcher;

    public class ViewModel {
        private var _eventDispatcher: EventDispatcher;

        public function ViewModel() {
            _eventDispatcher = new EventDispatcher();
        }
		
        public function addEventListener(event: String, listener: Function, ... params): void {
            var localListener: Function = listener;
            var localParams: Array = params;

            _eventDispatcher.addEventListener(event, function(e: ViewModelEvent): void {
                localListener.apply(null, localParams.concat(e.params));
            }, false, 0, false);
        }

        public function dispatchWith(eventType: String, ... params): void {
            _eventDispatcher.dispatchEvent(new ViewModelEvent(eventType, params));
        }
    }
}
