package src.Objects.Factories {

	import flash.display.DisplayObject;
	import flash.display.DisplayObjectContainer;
	import flash.filters.BlurFilter;
	import flash.geom.ColorTransform;
	import flash.geom.Matrix;
	import flash.geom.Rectangle;
	import flash.utils.getDefinitionByName;
	import src.Objects.AggressiveLazyValue;
	import src.Objects.Forest;

	/**
	 * ...
	 * @author Default
	 */
	public class ForestFactory {

		public function ForestFactory() {
		}

		public static function getSprite(lvl: int, centered: Boolean = false): DisplayObjectContainer
		{
			var objRef: Class = getDefinitionByName("FOREST_LVL_" + lvl) as Class;

			var sprite: DisplayObjectContainer = new objRef() as DisplayObjectContainer;

			if (centered)
			{
				var item: DisplayObject;
				for (var i: int = 0; i < sprite.numChildren; i++)
				{
					item = sprite.getChildAt(i);
					var rect: Rectangle = item.getRect(item);
					item.x -= rect.x;
					item.y -= rect.y;
				}
			}

			return sprite;
		}

		public static function getInstance(type: int, objX: int, objY: int, groupId: int, objectId: int, level: int): Forest
		{
			var forestObj: Forest = new Forest(type, objX, objY, groupId, objectId, level);

			forestObj.addChild(ObjectFactory.makeIntoShadow(getSprite(level)));
			forestObj.addChild(getSprite(level));

			return forestObj;
		}
	}
}

