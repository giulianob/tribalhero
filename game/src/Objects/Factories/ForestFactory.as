package src.Objects.Factories {

	import flash.display.DisplayObject;
	import flash.display.DisplayObjectContainer;
	import flash.geom.Rectangle;
	import flash.utils.getDefinitionByName;
	import src.Objects.Forest;
	import src.Objects.Troop.TroopObject;

	/**
	 * ...
	 * @author Default
	 */
	public class ForestFactory {

		public function ForestFactory() {
		}

		public static function getSprite(centered: Boolean = false): DisplayObjectContainer
		{
			var objRef: Class = getDefinitionByName("DEFAULT_FOREST") as Class;

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

		public static function getInstance(): Object
		{
			var obj:Object = getSprite();

			var forestObj: Forest = new Forest();
			forestObj.addChild(obj as DisplayObject);

			return forestObj;
		}
	}
}

