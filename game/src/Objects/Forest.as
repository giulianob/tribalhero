package src.Objects {

	import src.Objects.Factories.EffectReqFactory;
	import src.Objects.Factories.ForestFactory;
	import src.Objects.Factories.TroopFactory;
	import src.Objects.*;

	public class Forest extends GameObject {

		public var wood: AggressiveLazyValue;
		public var rate: Number;
		public var labor: int;
		public var depleteTime: int;
		
		public function Forest() {

		}
		
		public function ToSprite(): Object
		{
			return ForestFactory.getSprite(level);
		}
	}

}

