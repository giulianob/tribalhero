/**
 * ...
 * @author Default
 * @version 0.1
 */

package src.Objects.Prototypes {
	import src.Objects.Resources;
	import fl.lang.Locale;

	public class UnitPrototype {

		public var nlsname: String;
		public var type: int;
		public var level: int;
		public var baseClass: String;
		public var spriteClass: String;
		public var hp: int;
		public var attack: int;
		public var defense: int;
		public var vision: int;
		public var stealth: int;
		public var range: int;
		public var speed: int;
		public var trainResources: Resources = new Resources(0, 0, 0, 0, 0);
		public var trainTime: int;
		public var upgradeResources: Resources = new Resources(0, 0, 0, 0, 0);
		public var upgradeTime: int;
		public var weapon: String;
		public var unitClass: String;
		public var weaponClass: String;
		public var armor: String;
		public var upkeep: int;

		public function UnitPrototype() {

		}

		public function getName(): String
		{
			var str: String = Locale.loadString(nlsname + "_UNIT");
			if (str && str != "")
			return str;

			return "[" + nlsname + "]";
		}

		public function getDescription(): String
		{
			var str: String = Locale.loadString(nlsname + "_UNIT_DESC");
			if (str)
			return str;

			return "[" + nlsname + "_UNIT_DESC" + "]";
		}

		public static function sortOnTypeAndLevel(a:UnitPrototype, b:UnitPrototype):Number {
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

		public static function compareTypeAndLevel(a: UnitPrototype, value: Array):int
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

