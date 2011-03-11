/**
* ...
* @author Default
* @version 0.1
*/

package src.Objects {

	import flash.display.Bitmap;
	import flash.display.MovieClip;
	import flash.events.Event;
	import flash.events.EventDispatcher;
	import flash.events.TimerEvent;
	import flash.utils.Timer;
	import src.Constants;
	import src.Map.Camera;
	import src.UI.SmartMovieClip;
	
	public class SimpleObject extends SmartMovieClip implements IScrollableObject {
		
		public var objX: int;
		public var objY: int;
		
		public var bmpSprite: Bitmap;
		
		public function SimpleObject() {
			
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
