package src.Objects.Actions
{
	import src.Objects.Factories.StructureFactory;
	import src.Objects.Factories.WorkerFactory;
	import src.Objects.IObject;
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

		public override function toString() : String 
		{
			var workerPrototype: Worker = WorkerFactory.getPrototype(workerType);

			if (workerPrototype == null) return "[" + index + "]";
			
			var action: IAction = workerPrototype.getAction(index);

			if (action == null) return "Action";
			else return action.toString() + (count > 0 ? "(" + count + ")" : "");

			return "[" + index + "]";
		}

		public override function getType():int
		{
			var workerPrototype: Worker = WorkerFactory.getPrototype(workerType);

			var action: * = workerPrototype.getAction(index);

			if (action == null) return 0;
			
			return action.actionType;
		}
	}

}

