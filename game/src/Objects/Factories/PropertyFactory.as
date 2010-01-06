package src.Objects.Factories {
	import src.Map.Map;
	import src.Util.BinaryList.*;
	import src.Util.Util;
	import src.Objects.Prototypes.PropertyPrototype;
	
	/**
	* ...
	* @author Default
	*/
	public class PropertyFactory {
		
		private static var map: Map;
		private static var properties: BinaryList;
		
		public static function init(_map: Map, data: XML):void
		{
			map = _map;			
						
			properties = new BinaryList(PropertyPrototype.sortOnType, PropertyPrototype.compareType);
			
			for each (var propNode: XML in data.Property.*)						
				properties.add(new PropertyPrototype(propNode.@type, propNode.@name, propNode.@datatype), false);			
			
			properties.sort();
		}
		
		public static function getProperties(type: int): Array
		{
			return properties.getRange(type);
		}
		
	}
	
}