/**
* ...
* @author Default
* @version 0.1
*/

package src.Objects.Actions {
	import src.Objects.SimpleGameObject;

	public class CurrentAction {
		
		public var workerId: int;
		public var startTime: int;
		public var endTime: int;
		public var id: int;
		
		public function CurrentAction(workerId: int, id: int, startTime: int, endTime: int) 
		{
			this.workerId = workerId;
			this.id = id;
			this.startTime = startTime;
			this.endTime = endTime;
		}
		
		public static function sortOnId(a: *, b: *):Number 
		{
			var aId:Number = a.id;
			var bId:Number = b.id;
			
			if(aId > bId) {
				return 1;
			} else if(aId < bId) {
				return -1;
			} else  {
				return 0;
			}
		}
		
		public static function compareId(a: *, value: int):int
		{
			return a.id - value;
		}	
		
		public static function compareEndTime(a: *, value: int):int
		{
			return a.endTime - value;
		}	
		
		public function toString() : String
		{
			return "";
		}
		
		public function getType() : int 
		{
			return 0;
		}
		
		public function isCancellable() : Boolean 
		{
			return true;
		}
	}
	
}
