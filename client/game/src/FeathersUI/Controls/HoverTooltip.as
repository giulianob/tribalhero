package src.FeathersUI.Controls {
    import feathers.controls.Callout;

    import starling.display.DisplayObject;

    public class HoverTooltip extends ResponsiveTooltip {
        public function HoverTooltip(content: *, origin: DisplayObject, supportedDirections: String = Callout.DIRECTION_ANY, customCalloutFactory: Function = null) {
            super(content, origin, supportedDirections, customCalloutFactory, false, true);
        }
    }
}
