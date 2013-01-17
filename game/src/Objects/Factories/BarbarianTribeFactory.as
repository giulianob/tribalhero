package src.Objects.Factories {

	import flash.display.DisplayObject;
	import flash.display.DisplayObjectContainer;
	import flash.filters.BlurFilter;
	import flash.geom.ColorTransform;
	import flash.geom.Matrix;
	import flash.geom.Rectangle;
	import flash.utils.getDefinitionByName;
	import src.Global;
	import src.Objects.AggressiveLazyValue;
	import src.Objects.BarbarianTribe;
	import src.Objects.SimpleGameObject;
	import src.Objects.States.GameObjectState;

	public class BarbarianTribeFactory {

		public function BarbarianTribeFactory() {
		}

		public static function getSprite(centered: Boolean = false): DisplayObjectContainer
		{
			var objRef: Class = getDefinitionByName("STRONGHOLD_STRUCTURE") as Class;

			var sprite: DisplayObjectContainer = new objRef() as DisplayObjectContainer;

			if (centered)
			{
				sprite.getChildAt(0).x = 0;
				sprite.getChildAt(0).y = 0;
			}

			return sprite;
		}

		public static function getInstance(type: int, state: GameObjectState, objX: int, objY: int, groupId: int, objectId: int, level: int, count: int): SimpleGameObject
		{
			var obj: BarbarianTribe = new BarbarianTribe(type, state, objX, objY, groupId, objectId, level, count);

			obj.spriteContainer.addChild(ObjectFactory.makeIntoShadow(getSprite()));
			obj.spriteContainer.addChild(getSprite());

			obj.setOnSelect(Global.map.selectObject);
			
			return obj;
		}
	}
}

