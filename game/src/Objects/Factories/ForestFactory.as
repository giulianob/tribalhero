package src.Objects.Factories {

    import flash.display.Bitmap;
    import flash.display.DisplayObject;
    import flash.display.DisplayObjectContainer;
    import flash.display.Sprite;

    import src.Assets;
    import src.Constants;
    import src.Global;
    import src.Objects.Forest;
    import src.Objects.States.GameObjectState;

    public class ForestFactory {

		public function ForestFactory() {
		}

        private static function getSpriteName(): String {
            return "FOREST_LVL_4";
        }

        public static function getSprite(withPosition: String = "", withShadow: Boolean = false): DisplayObjectContainer
        {
            var assetName: String = getSpriteName();

            var image: DisplayObject = Assets.getInstance(assetName, withPosition);

            var sprite: Sprite = new Sprite();

            if (withShadow) {
                var shadow: Bitmap = Assets.getInstance(assetName + "_SHADOW", withPosition);
                shadow.alpha = Constants.shadowAlpha;
                shadow.name = "shadow";
                sprite.addChild(shadow);
            }

            sprite.addChild(image);

            return sprite;
        }

		public static function getInstance(type: int, state: GameObjectState, objX: int, objY: int, size: int, groupId: int, objectId: int): Forest
		{
			var forestObj: Forest = new Forest(type, state, objX, objY, size, groupId, objectId);

            forestObj.setSprite(getSprite("map"), Assets.getPosition(getSpriteName(), "map"));

			forestObj.setOnSelect(Global.map.selectObject);
			
			return forestObj;
		}
	}
}

