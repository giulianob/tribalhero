package src.Util
{
	import flash.text.*;
	import flash.utils.Timer;
	import flash.events.*;
	
	public class GeneralCounter extends TextField
	{
		private static const secs: int = 1;		
		private var i: int = 0;
		private var title: String = "";
		
		public function GeneralCounter(title: String)
		{
			this.title = title;
			autoSize = TextFieldAutoSize.LEFT;
			textColor = 0xFFFFFF;
			selectable = false;
			
			mouseEnabled = false;
			
			var textFormat: TextFormat = new TextFormat();
			textFormat.bold = true;
			textFormat.size = 14;			
			defaultTextFormat = textFormat;
		
			var timer:Timer = new Timer(secs * 1000);
			timer.addEventListener(TimerEvent.TIMER, updateText);
			timer.start();
			
			text = "? " + title;
		}
		
		public function updateCounter(): void
		{
			i++;
		}
		
		public function updateText(event: TimerEvent): void
		{
			var fps: Number = i / secs;
			i = 0;
			text = fps + " " + title;
		}
	}
}