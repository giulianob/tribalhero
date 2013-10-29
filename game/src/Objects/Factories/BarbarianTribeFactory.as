package src.Objects.Factories {

    import flash.display.Bitmap;
    import flash.display.DisplayObject;
    import flash.display.DisplayObjectContainer;
    import flash.display.Sprite;

    import src.Assets;
    import src.Constants;
    import src.Global;
    import src.Objects.BarbarianTribe;
    import src.Objects.SimpleGameObject;
    import src.Objects.States.GameObjectState;

    public class BarbarianTribeFactory {

		public function BarbarianTribeFactory() {
		}

        private static function getSpriteName(): String {
            return "BARBARIAN_TRIBE_STRUCTURE";
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

		public static function getInstance(type: int, state: GameObjectState, objX: int, objY: int, size: int, groupId: int, objectId: int, level: int, count: int): SimpleGameObject
		{
			var obj: BarbarianTribe = new BarbarianTribe(type, state, objX, objY, size, groupId, objectId, level, count);

            obj.setSprite(getSprite("map", true), Assets.getPosition(getSpriteName(), "map"));

			obj.setOnSelect(Global.map.selectObject);
			
			return obj;
		}
	}
}

