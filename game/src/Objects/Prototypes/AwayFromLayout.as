/**
 * ...
 * @author Default
 * @version 0.1
 */

package src.Objects.Prototypes {
	import src.Map.*;
	import src.Objects.Factories.StructureFactory;

	public class AwayFromLayout implements ILayout {

		public var type: int;
		public var minlevel: int;
		public var maxlevel: int;
		public var mindist: int;
		public var maxdist: int;
		public var compare: int;

		private var structPrototype: StructurePrototype;

		public function AwayFromLayout() {
		}

		public function validate(builder: CityObject, city: City, x: int, y: int): Boolean
		{		
			var radius: int = mindist;

			var effects: Array = builder.techManager.getAllEffects(EffectPrototype.INHERIT_SELF_ALL);
			for each (var effect: EffectPrototype in effects) {
				if (effect.effectCode == EffectPrototype.EFFECT_AWAY_FROM_STRUCTURE_MOD && effect.param1 == type) {
					radius += effect.param2;
				}
			}			

			var objects: Array = city.nearObjectsByRadius(0, radius, x, y, type);
			
			for each (var obj: CityObject in objects)
			{
				if (obj.level >= minlevel && obj.level <= maxlevel)
				return false;
			}

			return true;
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

			return "Away from all " + structPrototype.getName() + " (Lvl " + minlevel.toString() + "-" + maxlevel.toString() + ")";
		}
	}

}

