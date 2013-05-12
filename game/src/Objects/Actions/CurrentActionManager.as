package src.Objects.Actions {
	import src.Global;
	import src.Util.BinaryList.*;

	public class CurrentActionManager extends BinaryList
	{

		public function CurrentActionManager() {
			super(CurrentAction.sortOnId, CurrentAction.compareId);
		}

		public function hasAction(type: int, objId: int = 0): Boolean {
			for each(var currentAction: CurrentAction in this) {
				if (currentAction is CurrentActionReference) continue;		
				if (objId > 0 && objId != currentAction.workerId) continue;
				
				if (currentAction.getType() == type) return true;
			}

			return false;
		}
		
		public function getActions(type: int = 0): Array {
			var ret: Array = new Array();
			
			for each(var currentAction: CurrentAction in this) {
				if (currentAction is CurrentActionReference) continue;		
				
				if (type == 0 || currentAction.getType() == type) ret.push(currentAction);
			}

			return ret;			
		}

		public function getObjectActions(objId: int, activeOnly: Boolean = false) : Array {
			var ret: Array = new Array();
			for each(var currentAction: CurrentAction in this)
			{
				if (activeOnly && !(currentAction is CurrentActiveAction)) continue;
				if (currentAction.endTime - Global.map.getServerTime() <= 0) continue;				

				if (currentAction.workerId == objId) {
					ret.push(currentAction);
				}
			}

			return ret;
		}
	}

}

