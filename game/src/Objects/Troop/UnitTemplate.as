
package src.Objects.Troop {

	public class UnitTemplate {

		public var type: int;
		public var level: int;

		public function UnitTemplate(type: int, level: int) {
			this.type = type;
			this.level = level;
		}

		public static function sortOnType(a:UnitTemplate, b:UnitTemplate):Number
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

		public static function compareUnitType(a: UnitTemplate, value: int):int
		{
			return a.type - value;
		}
	}

}
