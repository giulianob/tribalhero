/**
* ...
* @author Default
* @version 0.1
*/

package src.Objects.Troop {

	public class Unit {
		
		public var type: int;
		public var count: int;
		
		public function Unit(type: int, count: int) 
		{
			this.type = type;
			this.count = count;
		}
		
		public static function sortOnType(a:Unit, b:Unit):Number 
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
		
		public static function compareUnitType(a: Unit, value: int):int
		{
			return a.type - value;
		}	
		
	}
	
}
