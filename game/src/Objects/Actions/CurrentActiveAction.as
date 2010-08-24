package src.Objects.Actions
{
	import src.Objects.Factories.StructureFactory;
	import src.Objects.Factories.WorkerFactory;
	import src.Objects.GameObject;
	import src.Objects.IObject;
	import src.Objects.Prototypes.StructurePrototype;
	import src.Objects.Prototypes.Worker;
	import src.Objects.StructureObject;

	public class CurrentActiveAction extends CurrentAction
	{
		public var index: int;
		public var count: int;
		
		public function CurrentActiveAction(workerId: int, id: int, index: int, count: int, startTime: int, endTime: int) 
		{
			super(workerId, id, startTime, endTime);
			this.index = index;
			this.count = count;
		}
		
		public override function toString(gameObject: IObject) : String {		
			var structPrototype: StructurePrototype = StructureFactory.getPrototype(gameObject.getType(), gameObject.getLevel());
			
			var workerPrototype: Worker;
			
			if (structPrototype)
				workerPrototype = WorkerFactory.getPrototype(structPrototype.workerid);			
			else
				return "[" + index + "]";
				
			var action: IAction = workerPrototype.getAction(index);
				
			if (action == null) 
				return "Action";
				
			if (workerPrototype)
				return action.toString() + (count > 0 ? "(" + count + ")" : "");
			
			return "[" + index + "]";	
		}
	}
	
}