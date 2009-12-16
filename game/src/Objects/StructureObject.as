package src.Objects {
	
	/**
	* ...
	* @author Default
	*/
	import src.Map.CityObject;
	import src.Objects.Prototypes.EffectPrototype;	
	import src.Objects.Factories.StructureFactory;
	import src.Objects.Prototypes.StructurePrototype;
	
	public class StructureObject extends GameObject {
			
		public var properties: Array = new Array();
		public var labor: int = 0;
		
		public function StructureObject() {
			
		}
		
		override public function setSelected(bool:Boolean = false):void 
		{
			super.setSelected(bool);
			
			var prototype: StructurePrototype = StructureFactory.getPrototype(type, level);
			if (prototype != null) {
				if (bool) showRadius(prototype.range);
				else hideRadius();
			}
		}
		
		public function clearProperties():void
		{
			properties = new Array();
		}
		
		public function addProperty(value: * ):void
		{
			properties.push(value);
		}	
		
		public function ToSprite(): Object
		{
			return StructureFactory.getSprite(type, level);
		}
	}
	
}