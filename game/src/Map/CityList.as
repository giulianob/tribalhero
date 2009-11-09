/**
* ...
* @author Default
* @version 0.1
*/

package src.Map {

	import flash.events.Event;
	import flash.events.EventDispatcher;
	import flash.events.IEventDispatcher;
	import src.Objects.UnitTemplate;
	import src.Util.BinaryList;
	import src.Util.Util;
	
	public class CityList extends BinaryList {
		public static const CITY_UPDATE: String = "CITY_UPDATE";
		
		private var cities: Array = new Array();
		
		public function CityList() {			
			super(City.sortOnId, City.compare);
		}
	
		public function getTemplateLevel(cityId: int, type: int): int
		{
			var city: City = get(cityId);
			
			if (city == null)
				return 1;
			
			return city.template.get(type).level;
		}
		
		override public function add(obj: *, resort: Boolean = true):void {			
			return super.add(obj, resort);
		}
		
		override public function clear():void {			
			return super.clear();
		}
		
		override public function remove(val:*):* 
		{
			return super.remove(val);
		}
		
		override public function removeByIndex(index:int):* 
		{			
			return super.removeByIndex(index);
		}
							
	}
	
}
