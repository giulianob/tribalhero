package src.Objects.Stronghold 
{
	import org.aswing.AsWingConstants;
	import org.aswing.Component;
	import org.aswing.JLabel;
	import src.Objects.Factories.StrongholdFactory;
	import src.Objects.SimpleGameObject;
	import src.Objects.SimpleObject;
	import src.Objects.States.GameObjectState;
	import src.UI.Components.RichLabel;
	import src.Util.StringHelper;
	import src.Util.Util;

	public class Stronghold extends SimpleGameObject
	{
		public static const BATTLE_STATE_NONE: int = 0;
		public static const BATTLE_STATE_GATE: int = 1;
		public static const BATTLE_STATE_MAIN: int = 2;
		
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
		
		public static function gateToString(level: int, value: int) : String {
			var limit: int = Stronghold.maxGateHp(level);
			return value.toString() + "/" + limit.toString();
		}
		
		public static function maxGateHp(level: int): int {
			return level * 500 + 5000;
		}
		
		public static function getBattleStateString(strongholdInfo: *, rows: int = 0, cols: int = 0): RichLabel {			
			if (strongholdInfo.battleState == BATTLE_STATE_MAIN) {
				return new RichLabel(StringHelper.localize("STRONGHOLD_STATE_MAIN_ATTACK", strongholdInfo.gateOpenTo.id, strongholdInfo.gateOpenTo.name, strongholdInfo.battleId), rows, cols);				
			}
			
			if (strongholdInfo.battleState == BATTLE_STATE_GATE) {
				return new RichLabel(StringHelper.localize("STRONGHOLD_STATE_GATE_ATTACK"), rows, cols);
			}
			
			return new RichLabel("", rows, cols);
		}
	}
}