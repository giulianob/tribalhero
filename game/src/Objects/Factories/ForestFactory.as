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
	import src.Objects.Forest;
	import src.Objects.States.GameObjectState;
    import src.Util.Util;

	/**
	 * ...
	 * @author Default
	 */
	public class ForestFactory {

		public function ForestFactory() {
		}

		public static function getSprite(centered: Boolean = false): DisplayObjectContainer
		{
			var objRef: Class = getDefinitionByName("FOREST_LVL_1") as Class;

			var sprite: DisplayObjectContainer = new objRef() as DisplayObjectContainer;

			if (centered)
			{
				Util.centerSprite(sprite);
			}

			return sprite;
		}

		public static function getInstance(type: int, state: GameObjectState, objX: int, objY: int, groupId: int, objectId: int): Forest
		{
			var forestObj: Forest = new Forest(type, state, objX, objY, groupId, objectId);

			forestObj.spriteContainer.addChild(ObjectFactory.makeIntoShadow(getSprite()));
			forestObj.spriteContainer.addChild(getSprite());

			forestObj.setOnSelect(Global.map.selectObject);
			
			return forestObj;
		}
	}
}

