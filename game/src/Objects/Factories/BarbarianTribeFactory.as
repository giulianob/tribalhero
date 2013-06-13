﻿package src.Objects.Factories {

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
	import src.Objects.BarbarianTribe;
	import src.Objects.SimpleGameObject;
	import src.Objects.States.GameObjectState;
    import src.Util.Util;

	public class BarbarianTribeFactory {

		public function BarbarianTribeFactory() {
		}

		public static function getSprite(): DisplayObjectContainer
		{
			var image: DisplayObject = new Assets.BARBARIAN_TRIBE_STRUCTURE() as DisplayObject;

            var sprite: Sprite = new Sprite();
            sprite.addChild(image);

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

