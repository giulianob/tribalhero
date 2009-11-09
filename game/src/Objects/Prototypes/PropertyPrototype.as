/**
* ...
* @author Default
* @version 0.1
*/

package src.Objects.Prototypes {
	import src.Objects.Resources;

	public class PropertyPrototype {
		
		public var type: int;
		public var name: String;
		public var datatype: String;
		
		public function PropertyPrototype(type: int, name: String, datatype: String) {
			this.type = type;
			this.name = name;
			this.datatype = datatype;
		}			
		
		public static function sortOnType(a:PropertyPrototype, b:PropertyPrototype):Number {
			var aType:Number = a.type;
			var bType:Number = b.type;
		
			if (aType > bType)
				return 1;
			else if (aType <bType)
				return -1;
			else
				return 0;			
		}
		
		public static function compareType(a: PropertyPrototype, value: int):int
		{
			return a.type - value;						
		}	
		
		public function toString(): String
		{
			return name + " " + type;
		}
	}
	
}
