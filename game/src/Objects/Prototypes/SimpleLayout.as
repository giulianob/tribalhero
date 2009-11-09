/**
* ...
* @author Default
* @version 0.1
*/

package src.Objects.Prototypes {
	import src.Map.*;
	import src.Objects.Factories.StructureFactory;
	import src.Objects.SimpleGameObject;
	import src.Util.Util;
	
	public class SimpleLayout implements ILayout {		

		public var type: int;
		public var minlevel: int;
		public var maxlevel: int;
		public var mindist: int;
		public var maxdist: int;
		public var compare: int;
		
		private var structPrototype: StructurePrototype;
		
		private var map: Map;
		
		public function SimpleLayout(map: Map) {
			this.map = map;
		}
		
		public function validate(map: Map, city: City, x: int, y: int): Boolean
		{			
			var objects: Array;
			if (mindist == -1)
				objects = city.objects.toArray();
			else
				objects = city.nearObjects(mindist, maxdist, x, y, type);
			
			for each (var obj: CityObject in objects)
			{
				if (obj.level >= minlevel && obj.level <= maxlevel)
					return true;
			}
			
			return false;
		}
		
		private function loadPrototype():void
		{
			if (structPrototype == null)
			{
				structPrototype = StructureFactory.getPrototype(type, minlevel);
			}
		}
		
		public function toString(): String
		{
			loadPrototype();
			
			var desc: String = "";
			
			if (mindist > -1)
			{
				if (maxdist < 5)
					desc += "Close ";
				else if (maxdist < 12)
					desc += "Nearby ";
			}
			
			var lvlReq: String = minlevel.toString();
				
			if (structPrototype != null)
				desc += structPrototype.getName() + " (Level " + lvlReq + ")";
			else
				desc += type.toString() + " (Level " + lvlReq + ")";
				
			return desc;
		}
	}

}