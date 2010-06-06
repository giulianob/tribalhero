package src.Objects.Factories {

	import flash.display.DisplayObject;
	import flash.display.DisplayObjectContainer;
	import flash.geom.Rectangle;
	import flash.utils.getDefinitionByName;
	import src.Objects.Troop.TroopObject;

	/**
	 * ...
	 * @author Default
	 */
	public class TroopFactory {

		public function TroopFactory() {
		}

		public static function getStateSprite(state: int): DisplayObjectContainer
		{
			var name: String = "";
			switch (state) {
				default:
					name = "TROOP_IDLE";
				break;
			}

			var objRef: Class = getDefinitionByName(name) as Class;

			var sprite: DisplayObjectContainer = new objRef() as DisplayObjectContainer;

			return sprite;
		}

		public static function getSprite(centered: Boolean = false): DisplayObjectContainer
		{
			var objRef: Class = getDefinitionByName("DEFAULT_TROOP") as Class;

			var sprite: DisplayObjectContainer = new objRef() as DisplayObjectContainer;

			if (centered)
			{
				// Troop objs only have 1 child
				sprite.getChildAt(0).x = 0;
				sprite.getChildAt(0).y = 0;
			}

			return sprite;
		}

		public static function getInstance(): Object
		{
			var obj:Object = getSprite();

			var troopObject: TroopObject = new TroopObject();
			troopObject.addChild(obj as DisplayObject);

			return troopObject;
		}
	}
}

