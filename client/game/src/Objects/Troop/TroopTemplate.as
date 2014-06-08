package src.Objects.Troop
{
    import src.Objects.Factories.UnitFactory;
    import src.Objects.Prototypes.UnitPrototype;

    /**
	 * ...
	 * @author Giuliano Barberi
	 */
	public class TroopTemplate
	{
		public var type: int;
		public var level: int;
		public var maxHp: Number;
		public var attack: Number;
		public var splash: int;
		public var defense: int;
		public var range: int;
		public var speed: int;
		public var stealth: int;

		public function TroopTemplate(type: int, level: int, maxHp: Number, attack: Number, splash: int, defense: int, range: int, speed: int, stealth: int)
		{
			this.type = type;
			this.level = level;
			this.maxHp = maxHp;
			this.attack = attack;
			this.splash = splash;
			this.defense = defense;
			this.range = range;
			this.speed = speed;
			this.stealth = stealth;
		}

		public function getPrototype() : UnitPrototype {
			return UnitFactory.getPrototype(type, level);
		}

		public static function sortOnType(a:TroopTemplate, b:TroopTemplate):Number
		{
			var aType:Number = a.type;
			var bType:Number = b.type;

			if(aType > bType) {
				return 1;
			} else if(aType < bType) {
				return -1;
			} else  {
				return 0;
			}
		}

		public static function compareUnitType(a: TroopTemplate, value: int):int
		{
			return a.type - value;
		}
	}

}
