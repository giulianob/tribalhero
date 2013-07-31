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
        //noinspection JSUnusedGlobalSymbols
        public var maxdist: int;
		public var compare: int;

		private var structPrototype: StructurePrototype;

		public function AwayFromLayout() {
		}

		public function validate(builder: CityObject, city: City, position: Position, size: int): Boolean
		{		
			var radius: int = mindist;
			var reduction: int = 0;
			
			var effects: Array = builder.techManager.getAllEffects(EffectPrototype.INHERIT_UPWARD);
			for each (var effect: EffectPrototype in effects) {
				if (effect.effectCode == EffectPrototype.EFFECT_AWAY_FROM_STRUCTURE_MOD && effect.param1 == type) {
					reduction = Math.min(int(effect.param2), reduction);
				}
			}			

			var objects: Array = city.nearObjectsByRadius(0, radius + reduction, position, size, type);
			
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

			var desc: String = "At least " + mindist.toString() + " radius from all other " + structPrototype.getName(true);
			
			if (minlevel > 1) {
				 desc += " (Lvl " + minlevel.toString() + "-" + maxlevel.toString() + ")";
			}
			
			return desc;
		}
	}

}

