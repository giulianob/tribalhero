package src.Objects.Factories {

	import flash.display.DisplayObject;
	import flash.display.DisplayObjectContainer;
	import flash.filters.BlurFilter;
	import flash.geom.ColorTransform;
	import flash.geom.Matrix;
	import flash.geom.Rectangle;
	import flash.utils.getDefinitionByName;
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

		public static function getInstance(lvl: int): Object
		{
			var obj:Object = getSprite(lvl);

			var forestObj: Forest = new Forest();

			var shadow: DisplayObjectContainer = getSprite(lvl);
			shadow.transform.colorTransform = new ColorTransform(0, 0, 0);
			shadow.transform.matrix = new Matrix(1, 0, -0.7, 0.5, 20, 15);
			shadow.alpha = 0.4;
			shadow.filters = [new BlurFilter(5, 5)];
			shadow.mouseEnabled = false;
			forestObj.addChild(shadow);

			forestObj.addChild(obj as DisplayObject);

			return forestObj;
		}
	}
}

