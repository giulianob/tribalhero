package src.Objects.Actions
{
	import src.Util.StringHelper;
	import src.Objects.GameObject;

	public class CurrentPassiveAction extends CurrentAction
	{
		public var type: int;
		public var nlsDescription: String;
		
		public function CurrentPassiveAction(workerId: int, id: int, type: int, nlsDescription: String, startTime: int, endTime: int) 
		{
			super(workerId, id, startTime, endTime);
			
			this.nlsDescription = nlsDescription;			
			this.type = type;
		}
		
		public override function toString() : String 
		{
			if (nlsDescription && nlsDescription != "") {
				var str: String = StringHelper.localize(nlsDescription);
				if (str && str != "")
					return str;

				return "[" + nlsDescription + "]";
			}
			else
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