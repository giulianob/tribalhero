package src.Objects {

	import flash.filters.GlowFilter;
	import flash.geom.Point;
	import src.Global;
	import src.Map.City;
	import src.Map.CityObject;
	import src.Objects.States.GameObjectState;

	public class GameObject extends SimpleGameObject implements IScrollableObject
	{
		public var playerId: int;				

		public function GameObject(type: int, state: GameObjectState, objX: int, objY: int, playerId: int, cityId: int, objectId: int)
		{
			super(type, state, objX, objY, cityId, objectId);			
			
			this.playerId = playerId;			
		}
		
		public function get cityId(): int		
		{
			return groupId;
		}
		
		public function getCorrespondingCityObj() : CityObject {
			var city: City = Global.map.cities.get(cityId);
			if (!city) return null;
			return city.objects.get(objectId);
		}		
	}
}

