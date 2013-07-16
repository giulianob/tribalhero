/**
 * ...
 * @author Default
 * @version 0.1
 */

package src.Objects.Prototypes {
    import src.Map.*;
    import src.Objects.Factories.StructureFactory;

    public class SimpleLayout implements ILayout {

		public var type: int;
		public var minlevel: int;
		public var maxlevel: int;
		public var mindist: int;
		public var maxdist: int;
		public var compare: int;

		private var structPrototype: StructurePrototype;

		public function SimpleLayout() {
		}

		public function validate(builder: CityObject, city: City, position: Position): Boolean
		{
			var objects: Array;
			if (mindist == -1) {
			    objects = city.objects.toArray();
            } else {
			    objects = city.nearObjectsByRadius(mindist, maxdist, position, type);
            }

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
				if (mindist <= 1) {
					desc += "Within " + maxdist + " radius of a ";
				} else {
					desc += "Between " + mindist + "-" + maxdist + " radius of a ";
				}
			}			

			if (structPrototype != null) {
				desc += structPrototype.getName();
			}
			else {
				desc += type.toString();
			}
			
			if (minlevel > 1) {
				 desc += " (Level " + minlevel.toString() + ")";
			}

			return desc;
		}
	}

}

