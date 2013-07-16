package src.Objects.Factories {

    import flash.display.DisplayObject;
    import flash.display.DisplayObjectContainer;
    import flash.display.Sprite;

    import src.Assets;
    import src.Global;
    import src.Objects.BarbarianTribe;
    import src.Objects.SimpleGameObject;
    import src.Objects.States.GameObjectState;

    public class BarbarianTribeFactory {

		public function BarbarianTribeFactory() {
		}

		public static function getSprite(withPosition: String = ""): DisplayObjectContainer
		{
			var image: DisplayObject = Assets.getInstance("BARBARIAN_TRIBE_STRUCTURE", withPosition);

            var sprite: Sprite = new Sprite();
            sprite.addChild(image);

			return sprite;
		}

		public static function getInstance(type: int, state: GameObjectState, objX: int, objY: int, size: int, groupId: int, objectId: int, level: int, count: int): SimpleGameObject
		{
			var obj: BarbarianTribe = new BarbarianTribe(type, state, objX, objY, size, groupId, objectId, level, count);

			obj.spriteContainer.addChild(ObjectFactory.makeIntoShadow(getSprite("map")));
			obj.spriteContainer.addChild(getSprite("map"));

			obj.setOnSelect(Global.map.selectObject);
			
			return obj;
		}
	}
}

