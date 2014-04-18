/**
 * ...
 * @author Default
 * @version 0.1
 */

package src.Objects.Prototypes {
    import src.Objects.Resources;
    import src.Util.StringHelper;

    public class UnitPrototype {

		public var nlsname: String;
		public var type: int;
		public var level: int;
		public var baseClass: String;
		public var spriteClass: String;
		public var carry: int;
		public var hp: Number;
		public var attack: Number;
		public var splash: int;
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

		public function getName(count: int = 1): String
		{
			var str: String = StringHelper.localize(nlsname + (count != 1 && count >= 0 ? "_PLURAL" : "") + "_UNIT");
			if (str && str != "")
			return str;

			return "[" + nlsname + "]";
		}

		public function getDescription(): String
		{
			var str: String = StringHelper.localize(nlsname + "_UNIT_DESC");
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

