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
			super(Troop.sortOnCityIdAndTroopId, Troop.compareCityIdAndTroopId);
			
			this.city = city;
		}
		
		public function getDefaultTroop(): Troop
		{
			return get([city.id, 1]);
		}			
	}	
}