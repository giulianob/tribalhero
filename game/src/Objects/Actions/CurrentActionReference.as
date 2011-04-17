/**
* ...
* @author Default
* @version 0.1
*/

package src.Objects.Actions {
	import src.Global;
	import src.Map.City;
	import src.Map.CityObject;
	import src.Objects.GameObject;
	import src.Objects.SimpleGameObject;

	public class CurrentActionReference {
		
		public var cityId: int;
		public var id: int;
		public var actionId: int;
		public var objectId: int;
		
		public function CurrentActionReference(cityId: int, id: int, objectId: int, actionId: int) 
		{
			this.cityId = cityId;
			this.id = id;
			this.actionId = actionId;
			this.objectId = objectId;
		}
		
		public function getAction() : CurrentAction
		{		
			var city: City = Global.map.cities.get(cityId);
			
			if (!city) return null;			
			
			return city.currentActions.get(actionId);
		}
		
		public function toString() : String
		{		
			var city: City = Global.map.cities.get(cityId);
			
			if (!city) return null;			
			
			return city.currentActions.get(actionId).toString();
		}
		
		public static function compareId(a: CurrentActionReference, value: int):int
		{
			return a.id - value;
		}	
		
		public static function sortOnId(a:CurrentActionReference, b:CurrentActionReference):Number 
		{
			var aId:Number = a.id;
			var bId:Number = b.id;
			
			if(aId > bId) {
				return 1;
			} else if(aId < bId) {
				return -1;
			} else  {
				return 0;
			}
		}		
	}
	
}
