package src.Objects.Actions {
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
