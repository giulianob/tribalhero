package src.UI.Tooltips {
	import src.Global;
	import src.Map.City;
	import src.Map.Username;
	import src.Objects.Actions.Notification;
	import src.Objects.Troop.*;
	import src.UI.Components.ComplexTroopGridList.ComplexTroopGridList;
	import src.UI.Components.NotificationBox;
	import src.UI.Components.ReferenceBox;
	import src.UI.LookAndFeel.GameLookAndFeel;

	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.geom.*;
	import org.aswing.colorchooser.*;
	import org.aswing.ext.*;

	public class TroopStubTooltip extends Tooltip {
		private var pnlHeader: JPanel;
		private var pnlTop: JPanel;
		private var lblName: JLabel;
		private var lblStatus: JLabel;
		private var troop: TroopStub;
		private var city: City;

		public function TroopStubTooltip(city: City, troop: TroopStub) {
			this.troop = troop;
			this.city = city;

			createUI();

			var notifications: Array = city.notifications.getByObject(troop.cityId, troop.objectId);
			for each (var notification: Notification in notifications) {
				ui.append(new NotificationBox(notification, true));
			}

			Global.map.usernames.cities.getUsername(troop.cityId, onReceiveCityUsername);
			lblStatus.setText(troop.getStateName());

			if (troop.getIndividualUnitCount() == 0) {
				var lblQuietHere: JLabel = new JLabel("It's quiet in here", null, AsWingConstants.LEFT);
				GameLookAndFeel.changeClass(lblQuietHere, "Tooltip.text");
				ui.append(lblQuietHere);
			}
			else {
				var tilelists: Array = ComplexTroopGridList.getGridList(troop, true);
				ui.append(ComplexTroopGridList.stackGridLists(tilelists, true));
			}

			ui.pack();
		}

		private function onReceiveCityUsername(username: Username, custom: *) : void {
			lblName.setText(username.name + troop.getNiceId(true));
		}

		private function createUI() : void {
			ui.setLayout(new SoftBoxLayout(AsWingConstants.VERTICAL, 5));
			ui.setMinimumWidth(150);

			pnlTop = new JPanel(new BorderLayout(20));

			lblName = new JLabel();
			lblName.setHorizontalAlignment(AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(lblName, "header");
			lblName.setConstraints("West");

			lblStatus = new JLabel();
			lblStatus.setHorizontalAlignment(AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(lblStatus, "Tooltip.text");
			lblStatus.setConstraints("East");

			pnlHeader = new JPanel(new SoftBoxLayout(AsWingConstants.VERTICAL, 0));
			pnlHeader.setBorder(null);

			pnlTop.append(lblName);
			pnlTop.append(lblStatus);

			ui.append(pnlTop);
		}

	}
}

