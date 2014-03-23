package src.Objects.Factories {
    import flash.display.DisplayObject;
    import flash.display.DisplayObjectContainer;
    import flash.display.Sprite;
    import flash.filters.BlurFilter;
    import flash.geom.ColorTransform;
    import flash.geom.Matrix;
    import flash.utils.getDefinitionByName;

    import src.Assets;

    import src.Map.Map;
    import src.Objects.BarbarianTribe;
    import src.Objects.Forest;
    import src.Objects.NewCityPlaceholder;
    import src.Objects.Prototypes.ObjectTypePrototype;
    import src.Objects.SimpleObject;
    import src.Objects.Stronghold.Stronghold;
    import src.Objects.StructureObject;
    import src.Objects.Troop.*;
    import src.Util.BinaryList.*;
    import src.Util.Util;

    public class ObjectFactory {

		public static const TYPE_CITY: int = 0;
		public static const TYPE_FOREST: int = 1;
		public static const TYPE_TROOP_OBJ: int = 2;
		public static const TYPE_UNIT: int = 3;
		public static const TYPE_STRUCTURE: int = 4;
		public static const TYPE_NEW_CITY_PLACEHOLDER: int = 5;
		public static const TYPE_STRONGHOLD: int = 6;
		public static const TYPE_BARBARIAN_TRIBE: int = 7;

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
			var ret: Array = [];

			for each (var obj: ObjectTypePrototype in objectTypes) {
				if (obj.name == name) {
					ret.push(obj.type);
				}
			}

			return ret;
		}
		
		public static function getFirstType(name: String) : int {
			for each (var obj: ObjectTypePrototype in objectTypes) {
				if (obj.name == name)
					return obj.type;
			}
			
			return -1;
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
			else if (type == 201)
				return TYPE_NEW_CITY_PLACEHOLDER;
			else if (type == 300)
				return TYPE_STRONGHOLD;
			else if (type == 400)
				return TYPE_BARBARIAN_TRIBE;
			else
				return TYPE_UNIT;
		}

		public static function getPrototype(type: int, level: int): *
		{
			switch(getClassType(type)) 
			{
				case TYPE_STRUCTURE:
					return StructureFactory.getPrototype(type, level);
				case TYPE_UNIT:
					return UnitFactory.getPrototype(type, level);
			}
		}

		public static function getSpriteEx(style: String, type:int, level:int, forDarkBackground:Boolean = false): DisplayObjectContainer
		{
            var sprite: DisplayObjectContainer;
			if (type >= 1000)
				sprite = StructureFactory.getSprite(style, type, level);
			else if (type == 100)
				sprite = TroopFactory.getSprite();
			else
				sprite = UnitFactory.getSprite(type, level, forDarkBackground);

            return sprite;
		}

		public static function getIcon(name: String) : DisplayObject
		{
			var objRef: Class;
			
			try {
				objRef = getDefinitionByName(name) as Class;
			}
			catch (error: Error)
			{
				return new Sprite();
			}

			return new objRef() as DisplayObject;
		}

		public static function getSprite(obj: SimpleObject, withPosition: String = ""):DisplayObjectContainer
		{
			var sprite: DisplayObjectContainer;
			if (obj is StructureObject) {
                var structure: StructureObject = StructureObject(obj);
				sprite = StructureFactory.getSprite(structure.style, structure.type, structure.level, withPosition);
            }
			else if (obj is TroopObject)
				sprite = TroopFactory.getSprite();
			else if (obj is Forest)
				sprite = ForestFactory.getSprite();
			else if (obj is NewCityPlaceholder)
				sprite = getNewCityPlaceholderSprite();
			else if (obj is Stronghold)
				sprite = StrongholdFactory.getSprite(withPosition);
			else if (obj is BarbarianTribe)
				sprite = BarbarianTribeFactory.getSprite();
			else
				return null;			

            return sprite;
		}

		public static function getNewCityPlaceholderSprite() : DisplayObjectContainer
		{
			var obj: Sprite = new Sprite();
			obj.addChild(Assets.getInstance("FOUNDATION"));
			return obj;
		}
		
		public static function getNewCityPlaceholderInstance(x: int, y: int) : NewCityPlaceholder
		{
			var obj: NewCityPlaceholder = new NewCityPlaceholder(x, y);
			obj.setSprite(Assets.getInstance("FOUNDATION", "map"), Assets.getPosition("FOUNDATION", "map"));
			return obj;
		}
	}

}

