/**
 * ...
 * @author Default
 * @version 0.1
 */

package src.Objects.Prototypes {
	import src.Util.StringHelper;
	import src.Global;
	import src.Map.City;
	import src.Map.CityObject;
	import src.Objects.Resources;
	import src.Objects.StructureObject;

	public class StructurePrototype {

		public var name: String;
		public var type: int;
		public var level: int;
		public var baseClass: String;
		public var spriteClass: String;
		public var hp: int;
		public var attack: int;
		public var splash: int;
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

		public var layouts: Array = [];

		public function StructurePrototype() {

		}

		public function addLayout(layout: ILayout):void
		{
			layouts.push(layout);
		}

		public function validateLayout(builder: CityObject, city: City, x: int, y: int): Boolean
		{
			if (!builder) return false;

			if (Global.map.regions.getObjectsAt(x, y, StructureObject).length > 0) {
				return false;
			}
			
			for each(var layout: ILayout in layouts)
			{
				if (!layout.validate(builder, city, x, y))
					return false;
			}

			return true;
		}

		public function getName(plural: Boolean = false): String
		{
			return StringHelper.localize(name + "_STRUCTURE_NAME" + (plural ? "_PLURAL" : ""));
		}

		public function getGeneralDescription(): String
		{
			return StringHelper.localize(name + "_STRUCTURE_DESCRIPTION");
		}

		public function getDescription(): String
		{
			return StringHelper.localize(name + "_STRUCTURE_LVL_" + level);
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

