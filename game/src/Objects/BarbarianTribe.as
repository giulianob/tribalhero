package src.Objects 
{
    import src.Objects.States.GameObjectState;

    public class BarbarianTribe extends SimpleGameObject
	{
		public var level: int;
		public var count: int;
        public var id: int;
		
		private var upkeepPerLevel: Array = [0, 10, 20, 41, 73, 117, 171, 237, 313, 401, 500,
											 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];

		public function BarbarianTribe(type: int, state: GameObjectState, objX: int, objY: int, size: int, groupId: int, objectId: int, level: int, count: int)
		{
			super(type, state, objX, objY, size, groupId, objectId);
			
            this.id = objectId;
			this.level = level;
			this.count = count;
		}
		
		public function upkeep() : int
		{
			return upkeepPerLevel[level];
		}
		
	}

}