package src.Objects {

    import src.Constants;
    import src.Objects.States.GameObjectState;

    public class Forest extends SimpleGameObject {

		public var wood: AggressiveLazyValue;
		public var depleteTime: int;
		
		public function Forest(type: int, state: GameObjectState, objX: int, objY: int, size: int,  groupId: int, objectId: int) {
			super(type, state, objX, objY, size, groupId, objectId);

	            mapPriority = Constants.mapObjectPriority.forest;
		}
	}

}

