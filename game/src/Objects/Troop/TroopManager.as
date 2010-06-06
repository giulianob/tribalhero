package src.Objects.Troop {
	import flash.events.EventDispatcher;
	import flash.events.IEventDispatcher;
	import flash.events.Event;
	import src.Map.City;
	import src.Util.BinaryList.*;	
	
	/**
	* ...
	* @author Default
	*/
	public class TroopManager extends BinaryList
	{	
		private var city: City;
		
		public function TroopManager(city: City) {			
			super(TroopStub.sortOnCityIdAndTroopId, TroopStub.compareCityIdAndTroopId);
			
			this.city = city;
		}
		
		public function getDefaultTroop(): TroopStub
		{
			return get([city.id, 1]);
		}			
	}	
}