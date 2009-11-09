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
		public var objectId: int;		
		
		public function CurrentActionReference(objectId: int, id: int) 
		{
			this.id = id;
			this.objectId = objectId;
		}
		
		public function getAction(gameObject: IObject) : CurrentAction
		{		
			var city: City = Global.map.cities.get(gameObject.getCityId());
			
			if (!city)
				return null;			
			
			return city.currentActions.get(id);									
		}
		
		public function toString(gameObject: IObject) : String
		{
			var defaultName: String = "Unknown Reference";
			
			var city: City = Global.map.cities.get(gameObject.getCityId());
			
			if (!city)
				return defaultName;			
			
			var currentAction: * = city.currentActions.get(id);
			
			if (!currentAction)
				return defaultName;
				
			var actionObj: CityObject = null;
			
			if (objectId != 0)			
				actionObj = city.objects.get(objectId);									
			
			if (!currentAction)
				return defaultName;
			
			return currentAction.toString(actionObj);	
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
