package src.Objects 
{
	import src.Objects.States.GameObjectState;
	/**
	 * ...
	 * @author Anthony Lam
	 */
	public class BarbarianTribe extends SimpleGameObject
	{
		public var level: int;
		public var count: int;
		
		public function BarbarianTribe(type: int, state: GameObjectState, objX: int, objY: int, groupId: int, objectId: int, level: int, count: int)
		{
			super(type, state, objX, objY, groupId, objectId);
			
			this.level = level;
			this.count = count;
		}
		
	}

}