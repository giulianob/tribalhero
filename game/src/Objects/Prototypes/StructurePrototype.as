/**
 * ...
 * @author Default
 * @version 0.1
 */

package src.Objects.Prototypes {
	import fl.lang.Locale;
	import src.Map.City;
	import src.Map.Map;
	import src.Objects.Resources;

	public class StructurePrototype {

		public var nlsname: String;
		public var type: int;
		public var level: int;
		public var baseClass: String;
		public var spriteClass: String;
		public var hp: int;
		public var attack: int;
		public var defense: int;
		public var radius: int;
		public var stealth: int;
		public var range: int;
		public var speed: int;
		public var buildResources: Resources = new Resources(0, 0, 0, 0, 0);
		public var buildTime: int;
		public var workerid: int;
		public var maxlabor: int;
		public var weapon: String;

		public var layouts: Array = new Array();

		public function StructurePrototype() {

		}

		public function addLayout(layout: ILayout):void
		{
			layouts.push(layout);
		}

		public function validateLayout(map: Map, city: City, x: int, y: int): Boolean
		{
			if (map.regions.getObjectAt(x, y) != null)
			return false;

			for each(var layout: ILayout in layouts)
			{
				if (!layout.validate(map, city, x, y))
				return false;
			}

			return true;
		}

		public function getName(): String
		{
			var str: String = Locale.loadString(spriteClass);
			if (str && str != "")
			return str;

			return "[" + nlsname + "]";
		}

		public function getDescription(): String
		{
			var str: String = Locale.loadString(spriteClass + "_LVL_" + level);
			if (str && str != "")
			return str;

			str = Locale.loadString(spriteClass + "_LVL_1");
			if (str && str != "")
			return str;

			return "";
		}

		public static function sortOnTypeAndLevel(a:StructurePrototype, b:StructurePrototype):Number {
			var aType:Number = a.type;
			var bType:Number = b.type;

			var aLevel:Number = a.level;
			var bLevel:Number = b.level;

			if (aType > bType)
			return 1;
			else if (aType <bType)
			return -1;
			else if (aLevel > bLevel)
			return 1;
			else if (aLevel < bLevel)
			return -1;
			else
			return 0;
		}

		public static function compareTypeAndLevel(a: StructurePrototype, value: Array):int
		{
			var typeDelta: int = a.type - value[0];
			var levelDelta: int = a.level - value[1];

			if (typeDelta != 0)
			return typeDelta;

			if (levelDelta != 0)
			return levelDelta;
			else
			return 0;
		}
	}

}

