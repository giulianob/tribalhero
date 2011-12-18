package src.UI.Tooltips {
	import src.Global;
	import src.Map.City;
	import src.Map.Username;
	import src.Objects.Actions.Notification;
	import src.Objects.Troop.*;
	import src.UI.Components.NotificationBox;
	import src.UI.LookAndFeel.GameLookAndFeel;

	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.geom.*;
	import org.aswing.colorchooser.*;
	import org.aswing.ext.*;

	public class NotificationTooltip extends Tooltip {
		private var lblName: JLabel;
		private var troop: TroopStub;
		private var city: City;
		private var notification: Notification;

		public function NotificationTooltip(city: City, notification: Notification) {
			this.notification = notification;
			this.city = city;

			createUI();

			Global.map.usernames.cities.getUsername(notification.cityId, onReceiveCityUsername);
		}

		private function onReceiveCityUsername(username: Username, custom: *) : void {
			lblName.setText(username.name);
		}

		private function createUI() : void {
			ui.setLayout(new SoftBoxLayout(AsWingConstants.VERTICAL, 10));
			ui.setMinimumWidth(200);

			lblName = new JLabel();
			lblName.setHorizontalAlignment(AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(lblName, "header");

			ui.appendAll(lblName, new NotificationBox(notification, true));
		}

	}
}

