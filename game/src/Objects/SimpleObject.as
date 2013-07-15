
package src.Objects {

    import com.greensock.TweenMax;

    import flash.display.DisplayObject;
    import flash.display.MovieClip;
    import flash.display.Sprite;
    import flash.events.Event;
    import flash.filters.GlowFilter;
    import flash.geom.Point;

    import src.Constants;
    import src.Map.ScreenPosition;

    public class SimpleObject extends MovieClip {
		
		public static const DISPOSED: String = "DISPOSED";
		
		public var disposed: Boolean;
				
		private var onSelect: Function;
		protected var selected: Boolean;						
		
		public var objectCount: DisplayObject;				
		
		// This is the container where the objects sprite/image should go
		public var spriteContainer: Sprite;

        public var primaryPosition: ScreenPosition = new ScreenPosition();
					
		public function SimpleObject(objX: int, objY: int) {
			super();
			
			spriteContainer = new Sprite();
			addChild(spriteContainer);
			
			this.objX = objX;
            this.objY = objY;
		}
		
		public function copy(obj: SimpleObject): void {
			for (var i: int = spriteContainer.numChildren - 1; i >= 0; i--) {
				spriteContainer.removeChildAt(i);				
			}
			
			for (i = obj.spriteContainer.numChildren - 1; i >= 0; i--) {
				spriteContainer.addChildAt(obj.spriteContainer.removeChildAt(i), 0);				
			}
			
			objX = obj.objX;
			objY = obj.objY;
			x = obj.x;
			y = obj.y;
		}
		
		public function setObjectCount(count: int) : void {
			if (objectCount != null) {
				removeChild(objectCount);			
				objectCount = null;
			}
			
			if (count <= 1) {
				return;		
			}
			
			var bubble: CountBubble = new CountBubble();
			bubble.mouseChildren = false;
			bubble.txtUnreadCount.mouseEnabled = false;
			bubble.txtUnreadCount.tabEnabled = false;
			bubble.txtUnreadCount.text = count.toString();
			bubble.x = Constants.tileW / 2;
			bubble.y = 0;
			
			objectCount = bubble;
			
			addChild(bubble);
		}
		
		public function dispose(): void {
			disposed = true;
			if (objectCount != null) {
				removeChild(objectCount);
				objectCount = null;
			}
			dispatchEvent(new Event(DISPOSED));
		}
		
		public function fadeIn():void
		{
			alpha = 0;
			TweenMax.to(this, 1, {alpha:1});
		}
			
		public function setOnSelect(callback: Function):void
		{
			onSelect = callback;
		}
		
		public function isSelectable(): Boolean {
			return onSelect != null;
		}

		public function setSelected(bool: Boolean = false):void
		{			
			if (bool) {
				filters = [new GlowFilter(0xFFFFFF, 0.5, 16, 16, 3)];
			}
			
			selected = bool;
			
			if (!bool) {
				setHighlighted(false);
			}
		}

		public function setHighlighted(bool: Boolean = false):void
		{
			if (selected)
				return;

			if (bool == false)			
				filters = [];			
			else			
				filters = [new GlowFilter(0xFFDD00, 0.5, 16, 16, 3)];			
		}		
		
		public function distance(x_1: int, y_1: int): int
		{
			var offset: int = 0;

			var objPos: Point = new Point();

			objPos.x = objX;
			objPos.y = objY;

			if (objPos.y % 2 == 1 && y_1 % 2 == 0 && x_1 <= objPos.x) offset = 1;
			if (objPos.y % 2 == 0 && y_1 % 2 == 1 && x_1 >= objPos.x) offset = 1;

			return ((x_1 > objPos.x ? x_1 - objPos.x : objPos.x - x_1) + (y_1 > objPos.y ? y_1 - objPos.y : objPos.y - y_1) / 2 + offset);
		}		
		
		public static function sortOnXandY(a: SimpleObject, b: SimpleObject):Number {
			var aX:Number = a.objX;
			var bX:Number = b.objX;

			var aY:Number = a.objY;
			var bY:Number = b.objY;
			
			if (aX > bX)
				return 1;
			else if (aX < bX)
				return -1;
			else if (aY > bY)
				return 1;
			else if (aY < bY)
				return -1;
			else
				return 0;
		}		
		
		public static function compareXAndY(a: SimpleObject, value: Array):int
		{
			var xDelta: int = a.objX - value[0];
			var yDelta: int = a.objY - value[1];
			
			if (xDelta != 0)
				return xDelta;
				
			if (yDelta != 0)
				return yDelta;
			else
				return 0;
		}
		
		public function get objX():int 
		{
			return primaryPosition.x;
		}
		
		public function set objX(value:int):void 
		{
			primaryPosition.x = value;
			x = value;
		}
		
		public function get objY():int 
		{
			return primaryPosition.y;
		}
		
		public function set objY(value:int):void 
		{
			primaryPosition.y = value;
            y = value;
		}
	}
	
}
