package src.Objects.Factories {

    import flash.display.Bitmap;
    import flash.display.DisplayObject;
	import flash.display.DisplayObjectContainer;
    import flash.display.Sprite;
    import flash.filters.BlurFilter;
	import flash.geom.ColorTransform;
	import flash.geom.Matrix;
	import flash.geom.Rectangle;
	import flash.utils.getDefinitionByName;
	import src.Global;
    import src.Assets;
    import src.Objects.AggressiveLazyValue;
	import src.Objects.Forest;
	import src.Objects.States.GameObjectState;
	import src.Objects.Stronghold.Stronghold;
    import src.Util.Util;

	public class StrongholdFactory {

		public function StrongholdFactory() {
		}

		public static function getSprite(withPosition: String): DisplayObjectContainer
		{
			var image: Bitmap = Assets.getInstance("STRONGHOLD_STRUCTURE");

            var sprite: Sprite = new Sprite();
            sprite.addChild(image);

			return sprite;
		}

		public static function getInstance(type: int, state: GameObjectState, objX: int, objY: int, groupId: int, objectId: int, level: int, tribeId: int): Stronghold
		{
			var strongholdObj: Stronghold = new Stronghold(type, state, objX, objY, groupId, objectId, level, tribeId);

			strongholdObj.spriteContainer.addChild(getSprite("map"));

			strongholdObj.setOnSelect(Global.map.selectObject);
			
			return strongholdObj;
		}
	}
}

