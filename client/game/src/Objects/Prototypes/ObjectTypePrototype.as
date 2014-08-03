/**
* ...
* @author Default
* @version 0.1
*/

package src.Objects.Prototypes {
	
	public class ObjectTypePrototype {
		
		public var name: String;
		public var type: int;
		
		public static function sortOnNameAndType(a:ObjectTypePrototype, b:ObjectTypePrototype):Number {
			var aName:String = a.name;
			var bName:String = b.name;

			var aType:Number = a.type;
			var bType:Number = b.type;
			
			if (aName > bName)
				return 1;
			else if (aName < bName)
				return -1;
			else if (aType > bType)
				return 1;
			else if (aType < bType)
				return -1;
			else
				return 0;			
		}
		
		public static function compareNameAndType(a: ObjectTypePrototype, value: Array):int
		{
			var aName:String = a.name;
			var bName:String = value[0];

			var aType:Number = a.type;
			var bType:Number = value[1];
			
			if (aName > bName)
				return 1;
			else if (aName < bName)
				return -1;
			else if (aType > bType)
				return 1;
			else if (aType < bType)
				return -1;
			else
				return 0;	
		}		
	}
	
}
