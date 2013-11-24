package src.Objects.Process 
{
	import src.Constants;
	import src.Global;
    import src.Objects.Actions.Notification;
	import src.Objects.Troop.TroopStub;
    import src.Util.Util;
	public class LocateNotificationProcess
	{
        private var notification:Notification;
		
		public function LocateNotificationProcess(notification: Notification) 
		{
				this.notification = notification;                
		}
				
		public function execute():void 
		{
			Global.map.selectWhenViewable(notification.cityId, notification.objectId);
			Global.mapComm.City.gotoNotificationLocation(notification.targetCityId, notification.cityId, notification.actionId);			
			Global.gameContainer.closeAllFrames();
		}
		
	}

}