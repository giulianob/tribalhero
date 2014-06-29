package src.Objects {

    import flash.filters.GlowFilter;

    import src.Constants;
    import src.Global;
    import src.Map.City;
    import src.Map.CityObject;
    import src.Objects.Factories.ObjectFactory;
    import src.Objects.States.GameObjectState;

    import starling.filters.BlurFilter;

    public class GameObject extends SimpleGameObject
	{
		public var playerId: int;				

		public function GameObject(type: int, state: GameObjectState, objX: int, objY: int, size: int, playerId: int, cityId: int, objectId: int)
		{
			super(type, state, objX, objY, size, cityId, objectId);

            mapPriority = Constants.mapObjectPriority.simpleGameObject;

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
			
			if (selected) {
				return;
            }
				
			if (!bool && Constants.session.playerId == playerId && ObjectFactory.isType("HighlightedObjects", type)) {
                disposeFilter();
                filter = BlurFilter.createGlow(0x00A2FF, 0.5, 1);
			}
		}
	}
}

