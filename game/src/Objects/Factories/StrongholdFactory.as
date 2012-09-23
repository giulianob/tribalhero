﻿package src.Objects.Factories {

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

	/**
	 * ...
	 * @author Default
	 */
	public class StrongholdFactory {

		public function StrongholdFactory() {
		}

		public static function getSprite(centered: Boolean = false): DisplayObjectContainer
		{
			var objRef: Class = getDefinitionByName("DEFAULT_STRUCTURE_SIMPLE") as Class;

			var sprite: DisplayObjectContainer = new objRef() as DisplayObjectContainer;

			if (centered)
			{
				sprite.getChildAt(0).x = 0;
				sprite.getChildAt(0).y = 0;
			}

			return sprite;
		}

		public static function getInstance(type: int, state: GameObjectState, objX: int, objY: int, groupId: int, objectId: int, level: int, tribeId: int): Stronghold
		{
			var strongholdObj: Stronghold = new Stronghold(type, state, objX, objY, groupId, objectId, level, tribeId);

			strongholdObj.spriteContainer.addChild(ObjectFactory.makeIntoShadow(getSprite()));
			strongholdObj.spriteContainer.addChild(getSprite());

			strongholdObj.setOnSelect(Global.map.selectObject);
			
			return strongholdObj;
		}
	}
}

