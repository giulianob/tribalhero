package src.Objects.Factories {

    import flash.display.DisplayObject;
    import flash.display.DisplayObjectContainer;
    import flash.display.Sprite;

    import src.Assets;
    import src.Global;
    import src.Objects.States.GameObjectState;
    import src.Objects.Troop.TroopObject;

    public class TroopFactory {

		public function TroopFactory() {
		}

        public static function getSprite(withPosition: String = ""): DisplayObjectContainer
		{
            var image: DisplayObject = Assets.getInstance("DEFAULT_TROOP", withPosition);

            var sprite: Sprite = new Sprite();
            sprite.addChild(image);

            return sprite;
		}

		public static function getInstance(type: int, state: GameObjectState, objX: int, objY: int, size: int, playerId: int, cityId: int, objectId: int): TroopObject
		{
			var troopObject: TroopObject = new TroopObject(type, state, objX, objY, size, playerId, cityId, objectId);
			
			troopObject.spriteContainer.addChild(getSprite("map"));
			
			troopObject.setOnSelect(Global.map.selectObject);
			
			return troopObject;
		}
	}
}

