package src.Objects.Actions
{
	import src.Objects.GameObject;
	import src.Objects.IObject;

	public class CurrentPassiveAction extends CurrentAction
	{
		public var type: int;
		
		public function CurrentPassiveAction(workerId: int, id: int, type: int, startTime: int, endTime: int) 
		{
			super(workerId, id, startTime, endTime);
			
			this.type = type;
		}
		
		public override function toString() : String 
		{
			return PassiveAction.toString(type);
		}
		
		public override function getType():int 
		{
			return type;
		}
		
		override public function isCancellable():Boolean 
		{
			return PassiveAction.isCancellable(type);
		}
	}
	
}