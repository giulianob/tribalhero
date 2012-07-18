package src.Objects.Stronghold 
{
	import src.Objects.Factories.StrongholdFactory;
	import src.Objects.SimpleGameObject;
	import src.Objects.States.GameObjectState;
	
	/**
	 * ...
	 * @author Anthony Lam
	 */
	public class Stronghold extends SimpleGameObject
	{
		public var level: int;
		public var tribeId: int;
		public function Stronghold(type: int, state: GameObjectState, objX: int, objY: int, groupId: int, objectId: int, level: int, tribeId: int) {
			super(type, state, objX, objY, groupId, objectId);
			this.level = level;
			this.tribeId = tribeId;
		}
		
		public function ToSprite(): Object
		{
			return StrongholdFactory.getSprite();
		}
		
	}

}