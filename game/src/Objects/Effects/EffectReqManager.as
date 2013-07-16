package src.Objects.Effects {
    import src.Objects.GameObject;
    import src.Objects.Prototypes.EffectReqPrototype;

    /**
	 * ...
	 * @author Default
	 */
	public class EffectReqManager {

		public var id: int;
		public var effectReqs: Array = [];

		public function EffectReqManager(id: int)
		{
			this.id = id;
		}

		public function validate(parentObj: GameObject, effects: Array): Array
		{
			var invalidEffects: Array = [];

			for each(var effect: EffectReqPrototype in effectReqs)
			{
				if (!RequirementFormula.validate(parentObj, effect, effects)) {
					invalidEffects.push(effect);
				}
			}

			return invalidEffects;
		}

		public static function sortOnId(a:EffectReqManager, b:EffectReqManager):Number {
			var aType:Number = a.id;
			var bType:Number = b.id;

			if (aType > bType)
			return 1;
			else if (aType <bType)
			return -1;
			else
			return 0;
		}

		public static function compareId(a: EffectReqManager, value: int):int
		{
			return a.id - value;
		}
	}

}

