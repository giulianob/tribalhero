package src.Util
{
	import flash.text.*;
	import flash.utils.Timer;
	import flash.events.*;
	
	public class FPSCounter extends TextField
	{
		private static const secs: int = 2;		
		private var i: int = 0;
		
		public function FPSCounter()
		{
			width = 100;
			textColor = 0xFFFFFF;
			selectable = false;
			
			mouseEnabled = false;
			
			var textFormat: TextFormat = new TextFormat();
			textFormat.bold = true;
			textFormat.size = 14;			
			defaultTextFormat = textFormat;

			addEventListener(Event.ENTER_FRAME, updateCounter);
			
			var timer:Timer = new Timer(secs * 1000);
			timer.addEventListener(TimerEvent.TIMER, updateText);
			timer.start();
			
			text = "? FPS";
		}
		
		public function updateCounter(event: Event): void
		{
			i++;
		}
		
		public function updateText(event: TimerEvent): void
		{
			var fps: Number = i / secs;
			i = 0;
			text = fps + " FPS";
		}
	}
}