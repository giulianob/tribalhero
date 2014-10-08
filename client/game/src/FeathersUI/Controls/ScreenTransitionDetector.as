package src.FeathersUI.Controls {
    import feathers.controls.*;
    import feathers.events.*;

    import starling.events.*;

    public class ScreenTransitionDetector extends EventDispatcher {
        private var _isTransitioning: Boolean;
        private var control: IScreen;
        private var owner: ScreenNavigator;

        public function ScreenTransitionDetector(control: IScreen) {
            this.control = control;

            this.control.addEventListener(Event.ADDED_TO_STAGE, addedToStage);
            this.control.addEventListener(Event.REMOVED_FROM_STAGE, removedFromStage);
        }

        private function addedToStage(event: Event): void {
            this.owner = control.owner;

            owner.addEventListener(FeathersEventType.TRANSITION_START, transitionStartHandler);
            owner.addEventListener(FeathersEventType.TRANSITION_COMPLETE, transitionCompleteHandler);
        }

        private function removedFromStage(event: Event): void {
            owner.removeEventListener(FeathersEventType.TRANSITION_START, transitionStartHandler);
            owner.removeEventListener(FeathersEventType.TRANSITION_COMPLETE, transitionCompleteHandler);
            owner = null;
        }

        public function dispose(): void {
            if (owner) {
                owner.removeEventListener(FeathersEventType.TRANSITION_START, transitionStartHandler);
                owner.removeEventListener(FeathersEventType.TRANSITION_COMPLETE, transitionCompleteHandler);
            }

            control.removeEventListener(Event.ADDED_TO_STAGE, addedToStage);
            control.removeEventListener(Event.REMOVED_FROM_STAGE, removedFromStage);
        }

        private function transitionStartHandler(event: Event): void {
            _isTransitioning = true;
            dispatchEventWith(FeathersEventType.TRANSITION_START);
        }

        private function transitionCompleteHandler(event: Event): void {
            _isTransitioning = false;
            dispatchEventWith(FeathersEventType.TRANSITION_COMPLETE);
        }

        public function get isTransitioning(): Boolean {
            return _isTransitioning;
        }
    }
}
