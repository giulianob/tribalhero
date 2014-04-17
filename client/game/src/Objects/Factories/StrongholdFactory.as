package src.Objects.Factories {

    import flash.display.Bitmap;
    import flash.display.DisplayObjectContainer;
    import flash.display.Sprite;

    import src.Assets;
    import src.Constants;
    import src.Global;
    import src.Objects.States.GameObjectState;
    import src.Objects.Stronghold.Stronghold;

    public class StrongholdFactory {

		public function StrongholdFactory() {
		}

        private static function getSpriteName(): String {
            return "DEFAULT_STRONGHOLD_STRUCTURE";
        }

		public static function getSprite(withPosition: String, withShadow: Boolean = false): DisplayObjectContainer
		{
			var image: Bitmap = Assets.getInstance(getSpriteName(), withPosition);

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

		public static function getInstance(type: int, state: GameObjectState, objX: int, objY: int, size: int, groupId: int, objectId: int, level: int, tribeId: int, gateMax: int): Stronghold
		{
			var strongholdObj: Stronghold = new Stronghold(type, state, objX, objY, size, groupId, objectId, level, tribeId, gateMax);

			strongholdObj.setSprite(getSprite("map", true), Assets.getPosition(getSpriteName(), "map"));

			strongholdObj.setOnSelect(Global.map.selectObject);
			
			return strongholdObj;
		}
	}
}

