package src.UI.Components 
{
	import flash.events.Event;
	
	public class RichLabelCustomEvent extends Event 
	{
		public static const CUSTOM_EVENT_MOUSE_OVER: String = "CUSTOM_EVENT_MOUSE_OVER";
		public static const CUSTOM_EVENT_CLICK: String = "CUSTOM_EVENT_CLICK";
		
		public var eventName:String;
		
		public function RichLabelCustomEvent(type: String, eventName: String) 
		{
			super(type);
			this.eventName = eventName;
		}
		
	}

}