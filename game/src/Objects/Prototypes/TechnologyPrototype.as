/**
 * ...
 * @author Default
 * @version 0.1
 */

package src.Objects.Prototypes {
	import src.Objects.Resources;
	import src.Util.StringHelper;
	import src.Util.BinaryList.*;

	public class TechnologyPrototype {

		public var level: int;
		public var resources: Resources;
		public var time: int;
		public var techtype: int;
		public var spriteClass: String;

		public var effects: BinaryList = new BinaryList(EffectPrototype.sortOnType, EffectPrototype.compare);

		public function TechnologyPrototype(techtype: int, level: int, resources: Resources, time: int, spriteClass: String) {
			this.level = level;
			this.resources = resources;
			this.time = time;
			this.techtype = techtype;
			this.spriteClass = spriteClass;
		}

		public function getName(): String
		{
			var str: String = StringHelper.localize(spriteClass + "_NAME");
			if (str && str != "")
			return str;

			return "[" + spriteClass + "_NAME]"; //this should be changed to nlsname
		}

		public function getDescription(): String {
			var str: String = StringHelper.localize(spriteClass + "_LVL_" + level);
			if (str && str != "")
			return str;

			return "[" + spriteClass + "_LVL_" + level + "]";
		}

		public static function sortOnTypeAndLevel(a:TechnologyPrototype, b:TechnologyPrototype):Number {
			var aType:Number = a.techtype;
			var bType:Number = b.techtype;

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

		public static function compareTypeAndLevel(a: TechnologyPrototype, value: Array):int
		{
			var typeDelta: int = a.techtype - value[0];
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

