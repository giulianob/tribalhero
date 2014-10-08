package src.FeathersUI.Map {
    import flash.geom.Point;
    import flash.geom.Rectangle;

    import src.Constants;

    import starling.display.DisplayObject;
    import starling.display.Sprite;

    public class MapOverlayBase extends Sprite {
        override public function hitTest(localPoint: Point, forTouch: Boolean = false): DisplayObject {
            if (forTouch && (!visible || !touchable)) return null;

            return this;
        }
    }
}
