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
	import src.Objects.IObject;
	import src.Objects.SimpleGameObject;

	public class CurrentActionReference {
		
		public var id: int;
		public var actionId: int;
		public var objectId: int;		
		
		public function CurrentActionReference(id: int, objectId: int, actionId: int) 
		{
			this.id = id;
			this.actionId = actionId;
			this.objectId = objectId;
		}
		
		public function getAction(gameObject: IObject) : CurrentAction
		{		
			var city: City = Global.map.cities.get(gameObject.getCityId());
			
			if (!city)
				return null;			
			
			return city.currentActions.get(actionId);									
		}
		
		public function toStringByCity(city: City) : String
		{
			var defaultName: String = "Unknown Reference";
			
			if (!city)
				return defaultName;			
			
			var currentAction: CurrentAction = city.currentActions.get(actionId);
			
			if (!currentAction)
				return defaultName;
				
			var actionObj: CityObject = null;
			
			if (currentAction.workerId != 0)			
				actionObj = city.objects.get(currentAction.workerId);
			
			if (!currentAction)
				return defaultName;
			
			return currentAction.toString(actionObj);	
		}
		
		public function toString(gameObject: IObject) : String
		{			
			var city: City = Global.map.cities.get(gameObject.getCityId());
			
			return toStringByCity(city);
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
