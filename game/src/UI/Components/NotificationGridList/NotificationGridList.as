package src.UI.Components.NotificationGridList 
{
	import flash.display.DisplayObject;
	import flash.events.Event;
	import flash.events.TimerEvent;
	import flash.utils.Timer;
	import org.aswing.event.AWEvent;
	import org.aswing.ext.DefaultGridCell;
	import org.aswing.ext.GeneralGridListCellFactory;
	import org.aswing.ext.GridList;
	import org.aswing.VectorListModel;
	import src.Map.City;
	import src.Map.CityObject;
	import src.Objects.Actions.Notification;
	import src.Objects.Actions.PassiveAction;
	import src.Util.BinaryList.*;
	
	/**
	 * ...
	 * @author Giuliano
	 */
	public class NotificationGridList extends GridList
	{
		private var city: City;
		private var timer: Timer;
		
		public function NotificationGridList(city: City, tileWidth: int) 
		{
			super(new VectorListModel(), new GeneralGridListCellFactory(NotificationGridCell), 0, 2);
			
			this.city = city;
			
			setTileWidth(tileWidth/2 - 30);
			setTileHeight(60);
			setColsRows(2, 0);
			setTracksWidth(true);
			
			onUpdateNotifications(null);
			
			timer = new Timer(1000);
			timer.addEventListener(TimerEvent.TIMER, function(e: TimerEvent):void { updateTimes(); } );
			timer.start();
			
			addEventListener(Event.ADDED_TO_STAGE, function(e: Event) : void {
				city.notifications.addEventListener(BinaryListEvent.CHANGED, onUpdateNotifications);
			});
			
			addEventListener(Event.REMOVED_FROM_STAGE, function(e: Event) : void {
				city.notifications.removeEventListener(BinaryListEvent.CHANGED, onUpdateNotifications);
				timer.stop();
			});
		}
		
		private function updateTimes(): void {
			for (var i: int = 0; i < cells.size(); i++) {
				var cell: NotificationGridCell = cells.get(i);
				cell.updateTime();
			}
		}
		
		public function onUpdateNotifications(e: Event): void {
			(getModel() as VectorListModel).clear();
			
			for each(var notification: Notification in city.notifications.each()) {
				(getModel() as VectorListModel).append( {'cityId': city.id, 'notification': notification, 'local': notification.cityId == city.id} );
			}
		}
	}
	
}