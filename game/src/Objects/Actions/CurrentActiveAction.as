package src.Objects.Actions
{
	import src.Objects.Factories.StructureFactory;
	import src.Objects.Factories.WorkerFactory;
	import src.Objects.Prototypes.StructurePrototype;
	import src.Objects.Prototypes.Worker;

	public class CurrentActiveAction extends CurrentAction
	{
		public var workerType: int;
		public var index: int;
		public var count: int;

		public function CurrentActiveAction(workerId: int, id: int, workerType: int, index: int, count: int, startTime: int, endTime: int) 
		{
			super(workerId, id, startTime, endTime);
			this.workerType = workerType;
			this.index = index;
			this.count = count;
		}

		public function getAction() : *
		{
			var workerPrototype: Worker = WorkerFactory.getPrototype(workerType);			
			var action: * = workerPrototype.getAction(index);
			
			return action;
		}
		
		override public function isCancellable():Boolean 
		{
			var action: Action = getAction();
			
			if (action == null) 
				return true;
			
			return !((action.options & Action.OPTION_UNCANCELABLE) == Action.OPTION_UNCANCELABLE);
		}
		
		public override function toString() : String 
		{		
			var action: IAction = getAction();

			if (action == null) 
				return "Missing Action";
			
			return action.toString() + (count > 0 ? "(" + count + ")" : "");			
		}

		public override function getType():int
		{
			var action: * = getAction();

			if (action == null) return 0;
			
			return action.actionType;
		}
	}

}

