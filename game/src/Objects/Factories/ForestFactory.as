package src.Objects.Factories {

    import flash.display.DisplayObject;
    import flash.display.DisplayObjectContainer;
    import flash.display.Sprite;

    import src.Assets;
    import src.Global;
    import src.Objects.Forest;
    import src.Objects.States.GameObjectState;

	public class ForestFactory {

		public function ForestFactory() {
		}

        public static function getSprite(lvl: int, withPosition: String = ""): DisplayObjectContainer
        {
            var image: DisplayObject = Assets.getInstance("FOREST_LVL_" + lvl, withPosition);

            var sprite: Sprite = new Sprite();
            sprite.addChild(image);

            return sprite;
        }

		public static function getInstance(type: int, state: GameObjectState, objX: int, objY: int, groupId: int, objectId: int, level: int): Forest
		{
			var forestObj: Forest = new Forest(type, state, objX, objY, groupId, objectId, level);

			forestObj.spriteContainer.addChild(ObjectFactory.makeIntoShadow(getSprite(level, "map")));
			forestObj.spriteContainer.addChild(getSprite(level, "map"));

			forestObj.setOnSelect(Global.map.selectObject);
			
			return forestObj;
		}
	}
}

