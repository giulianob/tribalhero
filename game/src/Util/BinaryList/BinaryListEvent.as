package src.Util.BinaryList 
{
	import flash.events.Event;
	
	public class BinaryListEvent extends Event
	{
		
		public static const CHANGED: String = "CHANGED";
		public static const ADDED: String = "ADDED";
		public static const REMOVED: String = "REMOVED";
		public static const UPDATED: String = "UPDATED";
		
		public var item: * = null;
			
		public function BinaryListEvent(type:String, item: * = null){
			super(type, false, false);
			this.item = item;
		}
			
		override public function clone() : Event{
			return new BinaryListEvent(type, item);
		}	
		
	}

}