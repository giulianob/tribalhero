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
	import src.Objects.Stronghold.Stronghold;
    import src.Util.Util;

	public class StrongholdFactory {

		public function StrongholdFactory() {
		}

		public static function getSprite(centered: Boolean = false): DisplayObjectContainer
		{
			var objRef: Class = getDefinitionByName("STRONGHOLD_STRUCTURE") as Class;

			var sprite: DisplayObjectContainer = new objRef() as DisplayObjectContainer;

			if (centered)
			{
				Util.centerSprite(sprite);
			}

			return sprite;
		}

		public static function getInstance(type: int, state: GameObjectState, objX: int, objY: int, groupId: int, objectId: int, level: int, tribeId: int, gateMax: int): Stronghold
		{
			var strongholdObj: Stronghold = new Stronghold(type, state, objX, objY, groupId, objectId, level, tribeId, gateMax);

			strongholdObj.spriteContainer.addChild(ObjectFactory.makeIntoShadow(getSprite()));
			strongholdObj.spriteContainer.addChild(getSprite());

			strongholdObj.setOnSelect(Global.map.selectObject);
			
			return strongholdObj;
		}
	}
}

