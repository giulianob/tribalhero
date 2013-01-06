package src.Objects.Actions 
{
	import src.Util.BinaryList.*;
	
	/**
	 * ...
	 * @author Giuliano
	 */
	public class NotificationManager extends BinaryList
	{
		
		public function NotificationManager() 
		{
			super(Notification.sortOnCityIdAndObjId, Notification.compareCityIdAndActionId);
		}
		
		public function getByObject(cityId: int, objectId: int) : Array {
			var ret: Array = new Array();
			
			for each (var notification: Notification in this) {
				if (notification.cityId == cityId && notification.objectId == objectId) 
					ret.push(notification);
			}
			
			return ret;
		}
		
	}
	
}