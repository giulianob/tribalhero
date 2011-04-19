/**
* ...
* @author Default
* @version 0.1
*/

package src.Objects {

	import flash.display.Bitmap;
	import flash.display.DisplayObject;
	import flash.display.MovieClip;
	import flash.events.Event;
	import flash.events.EventDispatcher;
	import flash.events.TimerEvent;
	import flash.filters.GlowFilter;
	import flash.geom.Point;
	import flash.utils.Timer;
	import src.Constants;
	import src.Map.Camera;
	import src.UI.SmartMovieClip;
	
	public class SimpleObject extends SmartMovieClip implements IScrollableObject {
		
		public var disposed: Boolean;
		
		public var objX: int;
		public var objY: int;
		
		private var onSelect: Function;
		private var selected: Boolean;						
		
		public var objectCount: DisplayObject;		
		
		public function SimpleObject() {
			addEventListener(Event.REMOVED_FROM_STAGE, function(e: Event) : void {
				if (objectCount != null) {
					removeChild(objectCount);
					objectCount = null;
				}
			});
		}
		
		public function setObjectCount(count: int) : void {
			if (objectCount != null) {
				removeChild(objectCount);			
				objectCount = null;
			}
			
			if (count <= 1) return;		
			
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
			if (objectCount) removeChild(objectCount);			
		}
		
		public function fadeIn():void
		{
			alpha = 0;
			var t:Timer = new Timer(50, 20);
			t.addEventListener(TimerEvent.TIMER, fadeInTimer);
			t.start();
		}
		
		private function fadeInTimer(event: TimerEvent):void
		{
			alpha += 0.05;
		}
		
		public function moveWithCamera(camera: Camera):void
		{
			x = objX - camera.x;
			y = objY - camera.y;
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
			filters = bool == false ? [] : [new GlowFilter(0xFFFFFF, 0.5, 16, 16, 3)];
			selected = bool;
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
		
		public function getX(): int
		{
			return objX;
		}
		
		public function getY(): int
		{
			return objY;
		}		
		
		public function setX(x: int):void
		{
			objX = x;
		}
		
		public function setY(y: int):void
		{
			objY = y;
		}
		
		public function setXY(x: int, y: int):void
		{
			setX(x);
			setY(y);
		}		
		
		public function distance(x_1: int, y_1: int): int
		{
			var offset: int = 0;

			var objPos: Point = new Point();

			objPos.x = getX();
			objPos.y = getY();

			if (objPos.y % 2 == 1 && y_1 % 2 == 0 && x_1 <= objPos.x) offset = 1;
			if (objPos.y % 2 == 0 && y_1 % 2 == 1 && x_1 >= objPos.x) offset = 1;

			return ((x_1 > objPos.x ? x_1 - objPos.x : objPos.x - x_1) + (y_1 > objPos.y ? y_1 - objPos.y : objPos.y - y_1) / 2 + offset);
		}		
		
		public static function sortOnXandY(a: IScrollableObject, b: IScrollableObject):Number {
			var aX:Number = a.getX();
			var bX:Number = b.getX();

			var aY:Number = a.getY();
			var bY:Number = b.getY();
			
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
		
		public static function compareXAndY(a: IScrollableObject, value: Array):int
		{
			var xDelta: int = a.getX() - value[0];
			var yDelta: int = a.getY() - value[1];
			
			if (xDelta != 0)
				return xDelta;
				
			if (yDelta != 0)
				return yDelta;
			else
				return 0;
		}			
	}
	
}
