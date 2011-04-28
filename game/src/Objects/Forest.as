package src.Objects {

	import src.Objects.*;
	import src.Objects.Factories.ForestFactory;
	import src.Objects.States.GameObjectState;

	public class Forest extends SimpleGameObject {

		public var wood: AggressiveLazyValue;
		public var level: int;
		public var rate: Number;
		public var labor: int;
		public var depleteTime: int;
		
		public function Forest(type: int, state: GameObjectState, objX: int, objY: int, groupId: int, objectId: int, level: int) {
			super(type, state, objX, objY, groupId, objectId);
			
			this.level = level;
		}
		
		public function ToSprite(): Object
		{
			return ForestFactory.getSprite(level);
		}
	}

}

