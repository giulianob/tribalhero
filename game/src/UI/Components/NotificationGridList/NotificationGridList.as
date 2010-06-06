package src.UI.Components.NotificationGridList
{
	import flash.events.Event;
	import flash.utils.Timer;
	import org.aswing.border.EmptyBorder;
	import org.aswing.ext.GeneralGridListCellFactory;
	import org.aswing.ext.GridList;
	import org.aswing.ext.GridListItemEvent;
	import org.aswing.Insets;
	import org.aswing.VectorListModel;
	import src.Global;
	import src.Map.City;
	import src.Objects.Actions.Notification;
	import src.UI.Tooltips.NotificationTooltip;
	import src.UI.Tooltips.Tooltip;
	import src.Util.BinaryList.*;
	import src.Util.Util;

	/**
	 * ...
	 * @author Giuliano
	 */
	public class NotificationGridList extends GridList
	{
		private var city: City;
		private var timer: Timer;
		private var tooltip: Tooltip;

		public function NotificationGridList(city: City)
		{
			super(new VectorListModel(), new GeneralGridListCellFactory(NotificationGridCell), 0, 2);

			this.city = city;

			setBorder(new EmptyBorder(null, new Insets(8, 3, 8, 3)));

			setTileWidth(32);
			setTileHeight(32);

			onUpdateNotifications(null);

			addEventListener(GridListItemEvent.ITEM_ROLL_OVER, onItemRollOver);
			addEventListener(GridListItemEvent.ITEM_ROLL_OUT, onItemRollOut);
			addEventListener(GridListItemEvent.ITEM_CLICK, onItemClick);

			addEventListener(Event.ADDED_TO_STAGE, function(e: Event) : void {
				city.notifications.addEventListener(BinaryListEvent.CHANGED, onUpdateNotifications);
			});

			addEventListener(Event.REMOVED_FROM_STAGE, function(e: Event) : void {
				city.notifications.removeEventListener(BinaryListEvent.CHANGED, onUpdateNotifications);
			});
		}

		private function onItemClick(event: GridListItemEvent) : void {
			var dp: NotificationGridCell = event.getCell() as NotificationGridCell;
			var value: * = dp.getCellValue();

			var notification: Notification = value.notification as Notification;

			Global.mapComm.City.gotoNotificationLocation(value.cityId, notification.cityId, notification.actionId);
			Global.map.selectWhenViewable(notification.cityId, notification.objectId);
			Util.getFrame(getParent()).dispose();
		}

		public function onItemRollOver(event: GridListItemEvent):void
		{
			var dp: NotificationGridCell = event.getCell() as NotificationGridCell;
			var value: * = dp.getCellValue();

			var notification: Notification = value.notification as Notification;
			
			tooltip = new NotificationTooltip(city, notification);
			tooltip.show(dp);
			
			this.tooltip = tooltip;
		}

		public function onItemRollOut(event: GridListItemEvent):void
		{
			if (tooltip) tooltip.hide();

			tooltip = null;
		}

		public function onUpdateNotifications(e: BinaryListEvent): void {
			(getModel() as VectorListModel).clear();

			for each(var notification: Notification in city.notifications.each()) {

				var local: Boolean = notification.cityId == city.id;
				// Don't show notifications from ourselves
				if (local) continue;

				(getModel() as VectorListModel).append( {'cityId': city.id, 'notification': notification, 'local': local} );
			}
		}
	}

}

