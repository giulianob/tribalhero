package src.UI.Dialog {
	import flash.display.*
	import org.aswing.AsWingConstants;
	import org.aswing.BorderLayout;
	import org.aswing.event.AWEvent;
	import org.aswing.FlowLayout;
	import org.aswing.JButton;
	import org.aswing.JFrame;
	import org.aswing.JLabel;
	import org.aswing.JPanel;
	import org.aswing.SoftBoxLayout;
	import src.Constants;
	import src.Global;
	import src.Map.*;
	import src.Objects.*;
	import src.Objects.Actions.Notification;
	import src.Objects.Troop.*;
	import src.UI.Components.ComplexTroopGridList.*;
	import src.UI.Components.NotificationBox;
	import src.UI.Components.SimpleTooltip;
	import src.UI.GameJPanel;
	import src.UI.LookAndFeel.GameLookAndFeel;
	import src.Util.BinaryList.BinaryListEvent;

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
			var buttons: Array = new Array();

			btnLocate.addActionListener(onClickLocate);
			btnRetreat.addActionListener(onClickRetreat);
			btnManage.addActionListener(onClickManage);

			if (troop.id == 1)
			buttons.push(btnManage);

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

			for each (var button: JButton in buttons)
			pnlButtons.append(button);

			//Notifications
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
			if (troop.state == TroopStub.MOVING || troop.state == TroopStub.RETURNING_HOME) {
				Global.map.selectWhenViewable(troop.cityId, troop.objectId);
			}

			Global.gameContainer.camera.ScrollTo(troop.x * Constants.tileW - Constants.screenW / 2, troop.y * Constants.tileH / 2 - Constants.screenH / 2);
			Global.gameContainer.closeAllFrames();
		}

		public function onClickRetreat(event: AWEvent):void
		{
			Global.mapComm.Troop.retreat(troop.cityId, troop.id);
		}

		public function onClickManage(e: AWEvent) :void
		{
			var unitMove: UnitMoveDialog = new UnitMoveDialog(city, onManage);
			unitMove.show();
		}

		public function onManage(dialog: UnitMoveDialog):void
		{
			dialog.getFrame().dispose();

			var troop: TroopStub = dialog.getTroop();
			if (troop.getIndividualUnitCount() == 0)
			return;

			Global.mapComm.Troop.moveUnit(city.id, troop);
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
			new SimpleTooltip(btnManage, "Move units between normal and garrison formations.\n\nUnits in the normal formation will defend your city if it is attacked. Units in garrison will hide while your city is being attacked.");

			append(pnlHeader);
		}
	}

}

