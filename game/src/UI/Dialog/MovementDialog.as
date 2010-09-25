﻿package src.UI.Dialog
{
	import flash.events.*;
	import org.aswing.event.*;
	import org.aswing.geom.IntDimension;
	import src.Constants;
	import src.Global;
	import src.Map.City;
	import src.Objects.Effects.Formula;
	import src.Objects.Factories.*;
	import src.UI.Components.NotificationGridList.NotificationGridList;
	import src.UI.Components.TroopStubGridList.TroopStubGridList;
	import src.UI.Cursors.*;
	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.geom.*;
	import org.aswing.colorchooser.*;
	import org.aswing.ext.*;
	import src.UI.*;
	import src.UI.LookAndFeel.GameLookAndFeel;
	import src.UI.Sidebars.CursorCancel.CursorCancelSidebar;
	import src.UI.Tooltips.*;
	import src.Util.BinaryList.BinaryListEvent;
	import src.Objects.Troop.*;

	public class MovementDialog extends GameJImagePanel
	{
		private var city: City;

		private var pnlContainer: JPanel;
		private var localTroopIcon: AssetPane;
		private var stationedHere: TroopStubGridList;
		private var stationedAway: TroopStubGridList;
		private var gridNotifications: NotificationGridList;
		private var myStubs: TroopStubGridList;
		private var btnAttack: JButton;
		private var btnDefend: JButton;
		private var lblCityName: JLabel;

		private var tooltip: Tooltip;

		public function MovementDialog(city: City)
		{
			super(new MOVEMENT_DIALOG_BG());

			this.city = city;

			createUI();

			btnAttack.addActionListener(onClickAttack);
			btnDefend.addActionListener(onClickReinforce);

			//Tooltip for local troop. Need to do it here since this guy is just a regular icon.
			//The other tooltips are handled by the grid lists
			localTroopIcon.addEventListener(MouseEvent.MOUSE_OVER, function(e: Event) : void {
				if (tooltip)
				tooltip.hide();

				tooltip = new TroopStubTooltip(city, city.troops.getDefaultTroop());
				tooltip.show(localTroopIcon);
			});

			localTroopIcon.addEventListener(MouseEvent.MOUSE_OUT, function(e: Event) : void {
				if (tooltip)
				tooltip.hide();

				tooltip = null;
			});

			localTroopIcon.addEventListener(MouseEvent.CLICK, function(e: Event) : void {
				var troopStubDialog: TroopStubDialog = new TroopStubDialog(city, city.troops.getDefaultTroop());
				troopStubDialog.show();
			});

			city.troops.addEventListener(BinaryListEvent.ADDED, onTroopAdded);
			city.troops.addEventListener(BinaryListEvent.REMOVED, onTroopRemoved);
			city.troops.addEventListener(BinaryListEvent.UPDATED, onTroopUpdated);

			for each (var troop: TroopStub in city.troops.each()) {
				addTroop(troop);
			}
		}

		public function onClickAttack(event: AWEvent):void
		{
			var attackTroopDialog: AttackTroopDialog = new AttackTroopDialog(city, city.troops.getDefaultTroop(), [Formation.Attack], onSendTroopAttack);
			attackTroopDialog.show();
		}

		public function onClickReinforce(event: AWEvent):void
		{
			var reinforceTroopDialog: ReinforceTroopDialog = new ReinforceTroopDialog(city, city.troops.getDefaultTroop(), [Formation.Defense], onSendTroopReinforce);
			reinforceTroopDialog.show();
		}

		public function onSendTroopReinforce(dialog: ReinforceTroopDialog):void
		{
			dialog.getFrame().dispose();
			getFrame().dispose();

			var troop: TroopStub = dialog.getTroop();
			if (troop.getIndividualUnitCount() == 0)
			return;

			var cursor: GroundReinforceCursor = new GroundReinforceCursor();

			cursor.init(troop, city.id);

			var sidebar: CursorCancelSidebar = new CursorCancelSidebar();
			Global.gameContainer.setSidebar(sidebar);
		}

		public function onSendTroopAttack(dialog: AttackTroopDialog):void
		{
			dialog.getFrame().dispose();
			getFrame().dispose();

			var troop: TroopStub = dialog.getTroop();
			if (troop.getIndividualUnitCount() == 0)
			{
				return;
			}

			var cursor: GroundAttackCursor = new GroundAttackCursor();

			cursor.init(troop, dialog.getMode(), city.id);

			var sidebar: CursorCancelSidebar = new CursorCancelSidebar();
			Global.gameContainer.setSidebar(sidebar);
		}

		private function onTroopUpdated(e: BinaryListEvent) : void {
			onTroopRemoved(e, false);
			onTroopAdded(e, false);
			updateLocalTroopIcon();
		}

		private function onTroopRemoved(e: BinaryListEvent, updateLocalTroop: Boolean = true) : void {
			stationedAway.removeStub(e.item);
			stationedHere.removeStub(e.item);
			myStubs.removeStub(e.item);
			updateLocalTroopIcon();
		}

		private function onTroopAdded(e: BinaryListEvent, updateLocalTroop: Boolean = true) : void {
			addTroop(e.item);
			updateLocalTroopIcon();
		}

		private function updateLocalTroopIcon() : void {
			localTroopIcon.setAsset(TroopFactory.getStateSprite(city.troops.getDefaultTroop().state, Formula.movementIconTroopSize(city.troops.getDefaultTroop())));
		}

		private function addTroop(troop: TroopStub) : void {
			if (troop.id == 1)
			return;

			//Add stationed troops to proper list
			if (troop.isStationed()) {
				if (troop.cityId != city.id) {
					stationedHere.addStub(troop);
				}
				else {
					stationedAway.addStub(troop);
				}
			}
			//Any other troop that isn't stationed and belongs to this city should go into the myStubs grid
			else if (troop.cityId == city.id) {
				myStubs.addStub(troop);
			}
		}

		public function show(owner:* = null, modal:Boolean = true, onClose: Function = null) : JFrame
		{
			super.showSelf(owner, modal, null, function() : void {
				city.troops.removeEventListener(BinaryListEvent.ADDED, onTroopAdded);
				city.troops.removeEventListener(BinaryListEvent.REMOVED, onTroopRemoved);
				city.troops.removeEventListener(BinaryListEvent.UPDATED, onTroopUpdated);
			});

			frame.setClosable(true);
			frame.setDragable(false);

			Global.gameContainer.showFrame(frame);

			return frame;
		}

		private function createUI() : void {
			title = "Unit Movement";

			setLayout(new EmptyLayout());
			setBorder(new EmptyBorder());

			setSize(new IntDimension(Math.min(976, Constants.screenW), Math.min(640, Constants.screenH - GameJImagePanelBackground.getFrameHeight())));

			pnlContainer = new JPanel(new EmptyLayout());
			pnlContainer.setSize(new IntDimension(976, 640));
			pnlContainer.scaleX = getSize().width / 976;
			pnlContainer.scaleY = getSize().height / 640;
			append(pnlContainer);

			lblCityName = new JLabel(city.name, null, AsWingConstants.LEFT);
			lblCityName.setLocation(new IntPoint(20, 450));
			lblCityName.setSize(new IntDimension(140, 40));
			GameLookAndFeel.changeClass(lblCityName, "darkHeader");
			pnlContainer.append(lblCityName);

			localTroopIcon = new AssetPane(TroopFactory.getStateSprite(city.troops.getDefaultTroop().state, Formula.movementIconTroopSize(city.troops.getDefaultTroop())));
			localTroopIcon.buttonMode = true;
			localTroopIcon.setBorder(null);
			localTroopIcon.pack();
			localTroopIcon.setLocation(new IntPoint(190, 420));
			pnlContainer.append(localTroopIcon);

			myStubs = new TroopStubGridList(city);
			myStubs.setBorder(null);
			myStubs.setTracksWidth(true);
			myStubs.setColumns(2);
			myStubs.setRows(0);

			var scrollMyStubs: JScrollPane = new JScrollPane(myStubs);
			scrollMyStubs.setLocation(new IntPoint(560, 480));
			scrollMyStubs.setSize(new IntDimension(350, 150));
			pnlContainer.append(scrollMyStubs);

			stationedHere = new TroopStubGridList(city);
			stationedHere.setBorder(null);
			stationedHere.setTracksWidth(true);
			stationedHere.setColumns(2);
			stationedHere.setRows(0);
			stationedHere.setAutoScroll(false);

			var scrollStationedHere: JScrollPane = new JScrollPane(stationedHere);
			scrollStationedHere.setLocation(new IntPoint(5, 480));
			scrollStationedHere.setSize(new IntDimension(350, 150));
			pnlContainer.append(scrollStationedHere);

			stationedAway = new TroopStubGridList(city);
			stationedAway.setBorder(null);
			stationedAway.setTracksWidth(true);
			stationedAway.setColumns(2);
			stationedAway.setRows(0);

			var scrollStationedAway: JScrollPane = new JScrollPane(stationedAway);
			scrollStationedAway.setLocation(new IntPoint(555, 215));
			scrollStationedAway.setSize(new IntDimension(350, 150));
			pnlContainer.append(scrollStationedAway);

			gridNotifications = new NotificationGridList(this.city);
			gridNotifications.setBorder(null);
			gridNotifications.setTracksWidth(true);
			gridNotifications.setColumns(3);
			gridNotifications.setRows(0);

			var scrollNotifications: JScrollPane = new JScrollPane(gridNotifications);
			scrollNotifications.setLocation(new IntPoint(120, 6));
			scrollNotifications.setSize(new IntDimension(510, 85));
			pnlContainer.append(scrollNotifications);

			btnAttack = new JButton("Send Attack");
			btnAttack.setLocation(new IntPoint(10, 0));
			btnAttack.setSize(new IntDimension(100, 25));
			pnlContainer.append(btnAttack);

			btnDefend = new JButton("Send Defense");
			btnDefend.setLocation(new IntPoint(10, 30));
			btnDefend.setSize(new IntDimension(100, 25));
			pnlContainer.append(btnDefend);
		}
	}
}

