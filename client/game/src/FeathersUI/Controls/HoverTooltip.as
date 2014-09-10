package src.FeathersUI.Controls {
    import feathers.controls.Callout;
    import feathers.controls.Label;
    import feathers.core.PopUpManager;

    import starling.display.DisplayObject;
    import starling.events.Event;
    import starling.events.Touch;
    import starling.events.TouchEvent;
    import starling.events.TouchPhase;

    public class HoverTooltip {
        private var content: DisplayObject;
        private var origin: DisplayObject;
        private var supportedDirections: String;
        private var customCalloutFactory: Function;
        private var callout: Callout;

        public function HoverTooltip(content:*, origin:DisplayObject, supportedDirections:String = Callout.DIRECTION_ANY, customCalloutFactory:Function = null) {
            if (content is String) {
                var text: String = content;
                content = new Label();
                content.text = text;
            }

            this.content = content;
            this.origin = origin;
            this.supportedDirections = supportedDirections;
            this.customCalloutFactory = customCalloutFactory;
        }

        public function bind(): void {
            origin.addEventListener(TouchEvent.TOUCH, onTouch);
            origin.addEventListener(Event.REMOVED_FROM_STAGE, onRemovedFromStage);
        }

        private function onRemovedFromStage(event: Event): void {
            if (callout) {
                callout.close(true);
            }

            origin.removeEventListener(TouchEvent.TOUCH, onTouch);
            origin.removeEventListener(Event.REMOVED_FROM_STAGE, onRemovedFromStage);
        }

        private function onTouch(event: TouchEvent): void {
            var hoverTouch: Touch = event.getTouch(origin, TouchPhase.HOVER);
            if (hoverTouch) {
                if (!callout) {
                    var factory:Function = customCalloutFactory;
                    if(factory == null)
                    {
                        factory = Callout.calloutFactory != null ? Callout.calloutFactory : Callout.defaultCalloutFactory;
                    }
                    callout = Callout(factory());
                    callout.content = content;
                    callout.supportedDirections = supportedDirections;
                    callout.origin = origin;
                    callout.disposeOnSelfClose = false;
                }

                // dont mess with callout if its already rendered
                if (callout.parent) {
                    return;
                }

                callout.close();
                PopUpManager.addPopUp(callout, false, false, null);
            }
            else {
                if (callout) {
                    callout.close();
                }
            }
        }
    }
}
