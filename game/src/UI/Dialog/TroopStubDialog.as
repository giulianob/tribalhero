package src.UI.Dialog {
	import flash.display.*;
	import org.aswing.*;
	import org.aswing.event.*;
	import src.*;
	import src.Map.*;
	import src.Objects.*;
	import src.Objects.Actions.*;
	import src.Objects.Process.*;
	import src.Objects.Troop.*;
	import src.UI.*;
	import src.UI.Components.*;
	import src.UI.Components.ComplexTroopGridList.*;
	import src.UI.LookAndFeel.*;
	import src.Util.BinaryList.*;

	public class TroopStubDialog extends GameJPanel {

		private var troop: TroopStub;

		private var pnlHeader: JPanel;
		private var pnlHeaderWest: JPanel;
		private var pnlButtons: JPanel;
		private var lblStatus: JLabel;

		private var btnLocate: JButton;
		private var btnRetreat: JButton;
		private var btnManage: JButton;

		private var city: City;

		public function TroopStubDialog(city: City, troop: TroopStub):void
		{
			title = "Troop Information";

			this.city = city;
			this.troop = troop;

			city.troops.addEventListener(BinaryListEvent.CHANGED, onUpdateTroop);
			city.notifications.addEventListener(BinaryListEvent.CHANGED, onNotificationsChanged);

			draw();
		}

		private function onNotificationsChanged(e: BinaryListEvent) : void {
			draw();
		}

		private function onUpdateTroop(e: BinaryListEvent) : void {
			var troop: TroopStub = city.troops.get([troop.cityId, troop.id]);
			if (!troop) {
				if (getFrame()) getFrame().dispose();
			}
			else {
				this.troop = troop;
				draw();
			}
		}

		private function draw() : void {
			createUI();

			//Buttons
			var buttons: Array = [];

			btnLocate.addActionListener(onClickLocate);
			btnRetreat.addActionListener(onClickRetreat);
			btnManage.addActionListener(onClickManage);

			if (troop.id == 1) {
				buttons.push(btnManage);
			}

			switch (troop.state)
			{
				case TroopStub.BATTLE:
					if (troop.id > 1) buttons.push(btnLocate);
				break;
				case TroopStub.BATTLE_STATIONED:
					buttons.push(btnLocate);
				break;
				case TroopStub.MOVING:
				case TroopStub.RETURNING_HOME:
					buttons.push(btnLocate);
				break;
				case TroopStub.STATIONED:
					buttons.push(btnLocate);
					buttons.push(btnRetreat);
				break;
			}

			for each (var button: JButton in buttons) pnlButtons.append(button);

			var notifications: Array = Global.gameContainer.selectedCity.notifications.getByObject(troop.cityId, troop.objectId);
			for each (var notification: Notification in notifications) {
				pnlHeaderWest.append(new NotificationBox(notification));
			}			

			//Formations
			var tilelists: Array = ComplexTroopGridList.getGridList(troop);
			append(ComplexTroopGridList.stackGridLists(tilelists));

			pack();

			if (getFrame()) getFrame().pack();
		}

		public function show(owner:* = null, modal:Boolean = true, onClose:Function = null):JFrame
		{
			super.showSelf(owner, modal, onClose, dispose);
			Global.gameContainer.showFrame(frame);

			Global.map.usernames.cities.getUsername(troop.cityId, onReceiveCityUsername);

			return frame;
		}

		private function dispose() : void {
			city.troops.removeEventListener(BinaryListEvent.CHANGED, onUpdateTroop);
			city.notifications.removeEventListener(BinaryListEvent.CHANGED, onNotificationsChanged);
		}

		private function onReceiveCityUsername(username: Username, custom: *) : void {
			getFrame().setTitle(username.name + troop.getNiceId(true));
		}

		public function onClickLocate(event: AWEvent):void
		{
			new LocateTroopProcess(troop).execute();
		}

		public function onClickRetreat(event: AWEvent):void
		{		
			new RetreatTroopProcess().execute();
		}

		public function onClickManage(e: AWEvent) :void
		{
			new ManageLocalTroopsProcess(city).execute();
		}

		private function createUI() : void {
			removeAll();

			setLayout(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 10));

			pnlHeader = new JPanel(new BorderLayout(5, 5));

			pnlHeaderWest = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 10));
			pnlHeader.setConstraints("West");

			pnlButtons = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 5));
			pnlButtons.setConstraints("East");

			lblStatus = new JLabel(troop.getStateName());
			lblStatus.setHorizontalAlignment(AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(lblStatus, "darkHeader");

			var pnlStatus: JPanel = new JPanel(new FlowLayout(AsWingConstants.LEFT, 0, 0, false));
			pnlStatus.append(new JLabel("This troop is currently ", null, AsWingConstants.LEFT));
			GameLookAndFeel.changeClass(pnlStatus.getComponent(0), "darkLargeText");
			pnlStatus.append(lblStatus);

			pnlHeaderWest.append(pnlStatus);

			pnlHeader.append(pnlHeaderWest);
			pnlHeader.append(pnlButtons);

			btnLocate = new JButton("Locate");
			btnRetreat = new JButton("Retreat");
			btnManage = new JButton("Manage");
			new SimpleTooltip(btnManage, "Move units between normal and hiding formations.\n\nUnits in the normal formation will defend your city if it is attacked. Units in hiding will not defend your city when attacked but will consume an extra 25% Upkeep.");

			append(pnlHeader);
		}
	}

}

