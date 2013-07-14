package src.UI.Components.TimePicker 
{
	import flash.events.Event;
	import flash.events.FocusEvent;
	import flash.events.KeyboardEvent;
	import org.aswing.JTextField;

    import src.Util.DateUtil;
    import src.Util.Util;
	
	/**
	 * ...
	 * @author Giuliano Barberi
	 */
	public class TimeTextField extends JTextField
	{
		
		protected var value: Number = 0;
		protected var isDirty: Boolean = false;
		
		public function TimeTextField(text: String = "0", columns: int = 3)
		{
			super();
			setRestrict("0-9:");
			addEventListener(KeyboardEvent.KEY_UP, onKeyUp);
			addEventListener(FocusEvent.FOCUS_OUT, onLoseFocus);
		}
		
		override public function setText(text:String):void 
		{			
			value = parseInt(text);			
			super.setText(DateUtil.formatTime(value));
		}
		
		override public function getText():String 
		{
			return value.toString();
		}		
		
		private function onKeyUp(e: Event = null): void
		{
			isDirty = true;
		}
		
		private function onLoseFocus(e: Event = null): void
		{
			if (!isDirty)
				return;
				
			var text: String = super.getText();
			var parts: Array = text.split(":", 3);
			
			var secondsPerUnit: Array = [ 3600, 60, 1 ];
			var time: int = 0;
			for (var i: int = 0; i < parts.length; i++)
			{
				var val: Number = parseInt(parts[i]);
				if (isNaN(val))
					val = 0;
					
				time += (secondsPerUnit[i] * val);
			}
			
			setText(time.toString());
		}
	}

}