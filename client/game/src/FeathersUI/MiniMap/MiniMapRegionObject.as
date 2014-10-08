package src.FeathersUI.MiniMap
{
    import flash.geom.Point;
    import flash.geom.Rectangle;

    import starling.display.*;

    import src.Constants;

    import src.Map.ScreenPosition;

    public class MiniMapRegionObject extends Sprite
	{
		public var type: int;
		public var groupId: int;
		public var objectId: int;

		public var extraProps: Object;
        public var size: int;
        public var position: ScreenPosition;
		
		public function MiniMapRegionObject(type: int, groupId: int, objectId: int, size: int, position: ScreenPosition, extraProps: Object) {
			this.type = type;
			this.groupId = groupId;
			this.objectId = objectId;
            this.size = size;
            this.position = position;
            this.x = position.x;
            this.y = position.y;
            this.extraProps = extraProps;
		}

        public function setIcon(sprite: DisplayObject): void {
            removeSprite();

            this.x = int(position.x - sprite.width/2 + (size-1) * Constants.miniMapTileW/2);
            this.y = int(position.y - sprite.height/4 + (size-1) * Constants.miniMapTileH/4);

            addChild(sprite);
        }

        public function removeSprite(): void {
            if (numChildren > 0) {
                removeChildren();
            }
        }

        public override function hitTest(localPoint:Point, forTouch:Boolean = false):DisplayObject {
            if (forTouch && (!visible || !touchable)) return null;

            var theBounds:Rectangle = getBounds(this);
            theBounds.inflate(5, 5);

            if (theBounds.containsPoint(localPoint)) return this;
            return null;
        }
    }
}
