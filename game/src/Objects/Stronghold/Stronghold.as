package src.Objects.Stronghold 
{
	import src.Objects.Factories.StrongholdFactory;
	import src.Objects.SimpleGameObject;
	import src.Objects.SimpleObject;
	import src.Objects.States.GameObjectState;

	public class Stronghold extends SimpleGameObject
	{
		public var level: int;
		public var tribeId: int;
		public var strongholdName: String;
		public var id: int;
		
		public function Stronghold(type: int, state: GameObjectState, objX: int, objY: int, groupId: int, objectId: int, level: int, tribeId: int) {
			super(type, state, objX, objY, groupId, objectId);
			this.id = objectId;
			this.level = level;
			this.tribeId = tribeId;
		}
		
		public function ToSprite(): Object
		{
			return StrongholdFactory.getSprite();
		}
		
		override public function copy(obj:SimpleObject):void 
		{
			super.copy(obj);
			var strongholdObj: Stronghold = obj as Stronghold;
			id = strongholdObj.id;
			level = strongholdObj.level;
			tribeId = strongholdObj.tribeId;
		}
		
		public static function GateToString(level: int, value: int) : String {
			var limit: int = level * 500 + 5000;
			return value.toString() + "/" + limit.toString();
		}
	}
}