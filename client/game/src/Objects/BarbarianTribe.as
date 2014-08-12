package src.Objects 
{
    import src.Constants;
    import src.Objects.Effects.Formula;
    import src.Objects.States.GameObjectState;

    public class BarbarianTribe extends SimpleGameObject
	{
		public var level: int;
		public var count: int;
        public var id: int;
		
		public function BarbarianTribe(type: int, state: GameObjectState, objX: int, objY: int, size: int, groupId: int, objectId: int, level: int, count: int)
		{
			super(type, state, objX, objY, size, groupId, objectId);

            mapPriority = Constants.mapObjectPriority.barbarianTribe;

            this.id = objectId;
			this.level = level;
			this.count = count;
		}
		
		public function upkeep() : int
		{
			return Formula.barbarianTribeUpkeep(level);
		}
		
	}

}