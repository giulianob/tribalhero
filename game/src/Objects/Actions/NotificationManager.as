package src.Objects.Actions 
{
	import src.Util.BinaryList;
	
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
		
	}
	
}