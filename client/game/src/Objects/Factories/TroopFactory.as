package src.Objects.Factories {

    import flash.geom.Point;

    import src.Global;
    import src.Objects.States.GameObjectState;
    import src.Objects.Troop.TroopObject;

    import starling.display.DisplayObjectContainer;

    public class TroopFactory {

		public function TroopFactory() {
		}

        public static function getSpriteName(): String {
            return "DEFAULT_TROOP";
        }

		public static function getInstance(type: int, state: GameObjectState, objX: int, objY: int, size: int, playerId: int, cityId: int, objectId: int): TroopObject
		{
            var typeName: String = getSpriteName();
            var position: Point = SpriteFactory.getMapPosition(typeName);
            var defaultSprite: DisplayObjectContainer = SpriteFactory.getStarlingImage(typeName);
			var troopObject: TroopObject = new TroopObject(type, state, defaultSprite, position, objX, objY, size, playerId, cityId, objectId);
            troopObject.setSprite(SpriteFactory.getStarlingImage(typeName), position);
			troopObject.setOnSelect(Global.map.selectObject);
			
			return troopObject;
		}
	}
}

