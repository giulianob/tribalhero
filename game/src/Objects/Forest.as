package src.Objects {

	import src.Objects.*;
	import src.Objects.Factories.ForestFactory;
	import src.Objects.States.GameObjectState;

	public class Forest extends SimpleGameObject {

		public var wood: AggressiveLazyValue;
		public var depleteTime: int;
		
		public function Forest(type: int, state: GameObjectState, objX: int, objY: int, groupId: int, objectId: int) {
			super(type, state, objX, objY, groupId, objectId);
		}
		
		public function ToSprite(): Object
		{
			return ForestFactory.getSprite();
		}
	}

}

