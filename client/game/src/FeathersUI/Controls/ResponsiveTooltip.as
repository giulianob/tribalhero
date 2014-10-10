package src.FeathersUI.Controls {
    import feathers.controls.Callout;
    import feathers.controls.Label;
    import feathers.core.PopUpManager;

    import src.Global;

    import starling.display.DisplayObject;
    import starling.events.Event;
    import starling.events.Touch;
    import starling.events.TouchEvent;
    import starling.events.TouchPhase;

    /*
    This is a tooltip that will show on touch if on a touch device or
    on hover if on desktop
     */
    public class ResponsiveTooltip {
        protected var content: *;
        protected var origin: DisplayObject;
        private var supportedDirections: String;
        private var customCalloutFactory: Function;
        protected var callout: Callout;
        protected var lbl: Label;
        private var disableEventsTime: Number;

        /*
        Content can either be a display object, a string, or a function
         */
        public function ResponsiveTooltip(content:*, origin:DisplayObject, supportedDirections:String = Callout.DIRECTION_ANY, customCalloutFactory:Function = null, touchSupport: Boolean = true, mouseSupport: Boolean = true) {
            this.content = content;
            this.origin = origin;
            this.supportedDirections = supportedDirections;
            this.customCalloutFactory = customCalloutFactory;
        }

        /*
        ########## Must call bind #############
         */
        public function bind(): void {
            origin.addEventListener(Event.ADDED_TO_STAGE, onAddedToStage);
        }

        private function onAddedToStage(event: Event = null): void {
            origin.addEventListener(TouchEvent.TOUCH, onTouch);
            origin.addEventListener(Event.REMOVED_FROM_STAGE, onRemovedFromStage);
        }

        private function onRemovedFromStage(event: Event): void {
            if (callout) {
                callout.close(true);
            }

            if (lbl) {
                lbl.dispose();
                lbl = null;
            }
            callout = null;

            origin.removeEventListener(TouchEvent.TOUCH, onTouch);
            origin.removeEventListener(Event.REMOVED_FROM_STAGE, onRemovedFromStage);

            origin.addEventListener(Event.ADDED_TO_STAGE, onAddedToStage);
        }

        private function onTouch(event: TouchEvent): void {
            var touch: Touch;
            if (Global.starlingStage.capabilities.isTouch) {
                touch = event.getTouch(origin, TouchPhase.BEGAN);
                if (touch) {
                    createCallout();
                    PopUpManager.addPopUp(callout, false, false, null);
                }
            }
            else {
                var hoverTouch: Touch = event.getTouch(origin, TouchPhase.HOVER);
                if (hoverTouch) {
                    if (new Date().time < disableEventsTime) {
                        return;
                    }

                    createCallout();

                    // dont mess with callout if its already rendered
                    if (!callout.parent) {
                        callout.close();
                        PopUpManager.addPopUp(callout, false, false, null);
                    }
                }
                else {
                    if (callout) {
                        callout.close(true);
                    }
                }

                if (callout) {
                    touch = event.getTouch(origin, TouchPhase.ENDED);
                    if (touch) {
                        callout.close(true);
                        // Disable events for a little bit of time so the callout doesnt show up again immediatelly
                        // if the origin was clicked
                        disableEventsTime = new Date().time + 150;
                    }
                }
            }
        }

        private function createCallout(): void {
            // check if this was auto disposed
            if (callout && !callout.parent) {
                callout = null;
            }

            if (!callout) {
                var factory: Function = customCalloutFactory;
                if (factory == null) {
                    factory = Callout.calloutFactory != null ? Callout.calloutFactory : Callout.defaultCalloutFactory;
                }
                callout = Callout(factory());

                var localContent: * = content is Function ? content() : content;
                if (localContent is String) {
                    if (!lbl) {
                        lbl = new Label()
                    }
                    lbl.text = localContent;
                    callout.content = lbl;
                }
                else {
                    callout.content = localContent;
                }

                callout.supportedDirections = supportedDirections;
                callout.origin = origin;
                callout.disposeOnSelfClose = true;
                callout.disposeContent = false;
            }
        }
    }
}
