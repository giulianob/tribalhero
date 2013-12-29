package src.Objects.Factories {

    import flash.display.Bitmap;
    import flash.display.DisplayObject;
    import flash.display.DisplayObjectContainer;
    import flash.display.Sprite;

    import src.Assets;
    import src.Constants;
    import src.Global;
    import src.Objects.States.GameObjectState;
    import src.Objects.Troop.TroopObject;

    public class TroopFactory {

		public function TroopFactory() {
		}

        public static function getSprite(withPosition: String = "", withShadow: Boolean = false): DisplayObjectContainer
		{
            var image: DisplayObject = Assets.getInstance(getSpriteName(), withPosition);

            var sprite: Sprite = new Sprite();

            if (withShadow) {
                var shadow: Bitmap = Assets.getInstance(getSpriteName() + "_SHADOW", withPosition);
                shadow.alpha = Constants.shadowAlpha;
                shadow.name = "shadow";
                sprite.addChild(shadow);
            }

            sprite.addChild(image);

            return sprite;
		}

        private static function getSpriteName(): String {
            return "DEFAULT_TROOP";
        }

		public static function getInstance(type: int, state: GameObjectState, objX: int, objY: int, size: int, playerId: int, cityId: int, objectId: int): TroopObject
		{
			var troopObject: TroopObject = new TroopObject(type, state, objX, objY, size, playerId, cityId, objectId);

			troopObject.setSprite(getSprite("map", true), Assets.getPosition(getSpriteName(), "map"));
			
			troopObject.setOnSelect(Global.map.selectObject);
			
			return troopObject;
		}
	}
}

