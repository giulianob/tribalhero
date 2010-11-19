package src.Objects.Actions {
	import src.Objects.IObject;
	import src.Util.BinaryList.*;

	/**
	 * ...
	 * @author Default
	 */
	public class CurrentActionManager extends BinaryList
	{

		public function CurrentActionManager() {
			super(CurrentAction.sortOnId, CurrentAction.compareId);
		}

		public function hasAction(type: int, gameObject: IObject): Boolean {
			for each(var currentAction: CurrentAction in each()) {
				if (currentAction is CurrentActionReference) continue;		
				
				if (currentAction.getType(gameObject) == type) return true;
			}

			return false;
		}

		public function getObjectActions(objId: int, activeOnly: Boolean = false) : Array {
			var ret: Array = new Array();
			for each(var currentAction: CurrentAction in each())
			{
				if (activeOnly && !(currentAction is CurrentActiveAction)) {
					continue;
				}

				if (currentAction.workerId == objId) {
					ret.push(currentAction);
				}
			}

			return ret;
		}
	}

}

