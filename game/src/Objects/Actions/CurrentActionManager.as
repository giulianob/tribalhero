package src.Objects.Actions {
	import flash.events.Event;
	import flash.events.EventDispatcher;
	import flash.events.IEventDispatcher;
	import src.Objects.SimpleGameObject;
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
		
		public function getObjectActions(objId: int) : Array {
			var ret: Array = new Array();
			for each(var currentAction: CurrentAction in each())
			{
				if (currentAction.workerId == objId) {
					ret.push(currentAction);
				}
			}
			
			return ret;
		}
	}
	
}