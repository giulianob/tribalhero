package src.Objects {
	import flash.events.EventDispatcher;
	import flash.events.IEventDispatcher;
	import flash.events.Event;
	import src.Util.BinaryList;	
	
	/**
	* ...
	* @author Default
	*/
	public class TroopManager extends BinaryList implements IEventDispatcher
	{	
		public function TroopManager() {			
			super(Troop.sortOnId, Troop.compareId);
		}
		
		public function getDefaultTroop(): Troop
		{
			return get(1);
		}			
		
		public function update(val:*):void
		{
			super.remove(val.id);
			super.add(val);
		}

	}	
}