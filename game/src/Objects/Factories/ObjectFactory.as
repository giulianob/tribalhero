package src.Objects.Factories {
	import flash.display.DisplayObject;
	import flash.display.DisplayObjectContainer;
	import flash.display.Sprite;
	import flash.filters.BlurFilter;
	import flash.geom.ColorTransform;
	import flash.geom.Matrix;
	import src.Map.Map;
	import src.Objects.Forest;
	import src.Objects.Prototypes.ObjectTypePrototype;
	import src.Objects.SimpleGameObject;
	import src.Objects.SimpleObject;
	import src.Objects.Factories.*;
	import src.Objects.StructureObject;
	import src.Objects.Troop.*;
	import src.UI.SmartMovieClip;
	import src.Util.BinaryList.*;

	import flash.utils.getDefinitionByName;

	public class ObjectFactory {

		public static const TYPE_UNIT: int = 1;
		public static const TYPE_STRUCTURE: int = 2;
		public static const TYPE_TROOP_OBJ: int = 3;
		public static const TYPE_FOREST: int = 4;

		private static var objectTypes: BinaryList;

		public static function init(_map: Map, data: XML):void
		{
			objectTypes = new BinaryList(ObjectTypePrototype.sortOnNameAndType, ObjectTypePrototype.compareNameAndType);

			for each (var objTypeNode: XML in data.ObjectTypes.*)
			{
				var objType: ObjectTypePrototype = new ObjectTypePrototype();
				objType.name = objTypeNode.@name;
				objType.type = objTypeNode.@type;

				objectTypes.add(objType, false);
			}

			objectTypes.sort();
		}

		public static function getList(name: String) : Array {
			var ret: Array = new Array();

			for each (var obj: ObjectTypePrototype in objectTypes.each()) {
				if (obj.name == name) {
					ret.push(obj.type);
				}
			}

			return ret;
		}

		public static function isType(name: String, type: int) : Boolean
		{
			return objectTypes.get([name, type]) != null;
		}

		public static function getClassType(type: int) : int
		{
			if (type >= 1000)
			return TYPE_STRUCTURE;
			else if (type == 100)
			return TYPE_TROOP_OBJ;
			else if (type == 200)
			return TYPE_FOREST;
			else
			return TYPE_UNIT;
		}

		public static function getPrototype(type: int, level: int): *
		{
			var classType: int = getClassType(type);

			if (classType == TYPE_STRUCTURE)
			return StructureFactory.getPrototype(type, level);
			else if (classType == TYPE_UNIT)
			return UnitFactory.getPrototype(type, level);
			else
			return null;
		}

		public static function getInstance(type: int, level: int): SimpleObject
		{
			var classType: int = getClassType(type);

			if (classType == TYPE_STRUCTURE)
			return StructureFactory.getInstance(type, level) as SimpleObject;
			else if (classType == TYPE_TROOP_OBJ)
			return TroopFactory.getInstance() as SimpleObject;
			else if (classType == TYPE_FOREST)
			return ForestFactory.getInstance(level) as SimpleObject;
			else
			return null;
		}

		public static function getSpriteEx(type: int, level: int, centered: Boolean = false): DisplayObjectContainer
		{
			if (type >= 1000)
			return StructureFactory.getSprite(type, level, centered);
			else if (type == 100)
			return TroopFactory.getSprite(centered);
			else
			return UnitFactory.getSprite(type, level);
		}

		public static function getIcon(name: String) : DisplayObjectContainer
		{
			var objRef: Class;

			try {
				objRef = getDefinitionByName(name) as Class;
			}
			catch (error: Error)
			{
				return new Sprite();
			}

			return new objRef() as DisplayObjectContainer;
		}

		public static function getSprite(obj: SimpleGameObject, centered: Boolean = false, encased: Boolean = false):DisplayObjectContainer
		{
			var sprite: DisplayObjectContainer;
			if (obj is StructureObject) {
				sprite = StructureFactory.getSprite((obj as StructureObject).type, (obj as StructureObject).level, centered);
			} else if (obj is TroopObject) {
				sprite = TroopFactory.getSprite(centered);
			} else if (obj is Forest) {
				sprite = ForestFactory.getSprite(obj.level, centered);
			} else {
				return null;
			}

			if (encased)
			{
				var holder: SmartMovieClip = new SmartMovieClip();
				holder.addChild(sprite);
				return holder;
			}
			else
			return sprite;
		}

		public static function getSimpleGameObject(name: String): SimpleGameObject {
			var obj: SimpleGameObject = new SimpleGameObject();

			var objRef: Class = getDefinitionByName(name) as Class;
			obj.addChild(new objRef() as DisplayObject);

			return obj;
		}

		public static function getSimpleObject(name: String, addShadow: Boolean = true): SimpleObject {
			var obj: SimpleObject = new SimpleObject();
			var objRef: Class = getDefinitionByName(name) as Class;

			if (addShadow) {
				var shadow: DisplayObjectContainer = new objRef() as DisplayObjectContainer;
				makeIntoShadow(shadow);
				obj.addChild(shadow);
			}

			obj.addChild(new objRef() as DisplayObject);

			return obj;
		}

		public static function makeIntoShadow(shadow: DisplayObjectContainer) : DisplayObjectContainer {
			shadow.transform.colorTransform = new ColorTransform(0, 0, 0);
			shadow.transform.matrix = new Matrix(1, 0, -0.7, 0.5, 20, 15);
			shadow.alpha = 0.4;
			shadow.filters = [new BlurFilter(5, 5)];
			shadow.mouseEnabled = false;

			return shadow;
		}
	}

}

