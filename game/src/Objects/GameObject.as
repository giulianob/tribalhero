package src.Objects {

	import flash.filters.GlowFilter;
	import flash.geom.Point;
	import src.Constants;
	import src.Global;
	import src.Map.City;
	import src.Map.CityObject;
	import src.Objects.Factories.ObjectFactory;
	import src.Objects.States.GameObjectState;

	public class GameObject extends SimpleGameObject
	{
		public var playerId: int;				

		public function GameObject(type: int, state: GameObjectState, objX: int, objY: int, playerId: int, cityId: int, objectId: int)
		{
			super(type, state, objX, objY, cityId, objectId);			
			
			this.playerId = playerId;			
			
			setHighlighted(false);
		}
		
		override public function copy(obj:SimpleObject):void 
		{
			super.copy(obj);
			var gameObj: GameObject = obj as GameObject;
			playerId = gameObj.playerId;
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
		
		override public function setHighlighted(bool:Boolean = false):void 
		{
			super.setHighlighted(bool);
			
			if (selected)
				return;
				
			if (!bool && Constants.playerId == playerId && ObjectFactory.isType("HighlightedObjects", type)) {
				filters = [new GlowFilter(0x00A2FF, 0.5, 16, 16, 2)];			
			}
		}
	}
}

