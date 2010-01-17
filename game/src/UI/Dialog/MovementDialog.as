package src.UI.Dialog
{
	import flash.events.Event;
	import flash.events.MouseEvent;
	import org.aswing.AssetPane;
	import org.aswing.EmptyLayout;
	import org.aswing.event.AWEvent;
	import org.aswing.geom.IntDimension;
	import src.Global;
	import src.Map.City;
	import src.Objects.Factories.*;
	import src.UI.Components.NotificationGridList.NotificationGridList;
	import src.UI.Components.TroopStubGridList.TroopStubGridList;
	import src.UI.Cursors.GroundAttackCursor;
	import src.UI.Cursors.GroundReinforceCursor;
	import src.UI.GameJPanel;
	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.geom.*;
	import org.aswing.colorchooser.*;
	import org.aswing.ext.*;
	import src.UI.Sidebars.CursorCancel.CursorCancelSidebar;
	import src.UI.Tooltips.Tooltip;
	import src.UI.Tooltips.TroopStubTooltip;
	import src.Util.BinaryList.BinaryListEvent;
	import src.Objects.Troop.*;

	public class MovementDialog extends GameJPanel
	{
		private var city: City;

		private var localTroopIcon: AssetPane;
		private var stationedHere: TroopStubGridList;
		private var stationedAway: TroopStubGridList;
		private var gridNotifications: NotificationGridList;
		private var myStubs: TroopStubGridList;
		private var btnAttack: JButton;
		private var btnDefend: JButton;

		private var tooltip: Tooltip;

		public function MovementDialog(city: City)
		{
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
			Global.map.gameContainer.setSidebar(sidebar);
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
			Global.map.gameContainer.setSidebar(sidebar);
		}

		private function onTroopUpdated(e: BinaryListEvent) : void {
			onTroopRemoved(e);
			onTroopAdded(e);
		}

		private function onTroopRemoved(e: BinaryListEvent) : void {
			stationedAway.removeStub(e.item);
			stationedHere.removeStub(e.item);
			myStubs.removeStub(e.item);
		}

		private function onTroopAdded(e: BinaryListEvent) : void {
			addTroop(e.item);
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

			});

			Global.gameContainer.showFrame(frame);
			frame.setTitle("Unit Movement");
			return frame;
		}

		private function createUI() : void {
			var layout: EmptyLayout = new EmptyLayout();
			setLayout(layout);
			setSize(new IntDimension(858, 600));
			setBorder(null);

			var bg: AssetPane = new AssetPane(new MOVEMENT_DIALOG_BG());
			bg.setBorder(null);
			bg.pack();
			bg.setLocation(new IntPoint(0, 5));
			append(bg);

			localTroopIcon = new AssetPane(new DEFAULT_TROOP());
			localTroopIcon.buttonMode = true;
			localTroopIcon.setBorder(null);
			localTroopIcon.pack();
			localTroopIcon.setLocation(new IntPoint(440, 175));
			append(localTroopIcon);

			myStubs = new TroopStubGridList(city);
			myStubs.setBorder(null);
			myStubs.setLocation(new IntPoint(70, 300));
			myStubs.setSize(new IntDimension(100, 210));
			myStubs.setTracksWidth(true);
			append(myStubs);

			stationedHere = new TroopStubGridList(city);
			stationedHere.setBorder(null);
			stationedHere.setLocation(new IntPoint(275, 30));
			stationedHere.setSize(new IntDimension(410, 110));
			stationedHere.setTracksWidth(true);
			append(stationedHere);

			stationedAway = new TroopStubGridList(city);
			stationedAway.setBorder(null);
			stationedAway.setLocation(new IntPoint(20, 550));
			stationedAway.setSize(new IntDimension(410, 38));
			stationedAway.setTracksWidth(true);
			append(stationedAway);

			gridNotifications = new NotificationGridList(this.city);
			gridNotifications.setBorder(null);
			gridNotifications.setLocation(new IntPoint(550, 300));
			gridNotifications.setSize(new IntDimension(100, 210));
			gridNotifications.setTracksWidth(true);
			append(gridNotifications);
			
			btnAttack = new JButton("Send Attack");
			btnAttack.setLocation(new IntPoint(10, 10));
			btnAttack.setSize(new IntDimension(100, 25));
			append(btnAttack);

			btnDefend = new JButton("Send Defense");
			btnDefend.setLocation(new IntPoint(10, 40));
			btnDefend.setSize(new IntDimension(100, 25));
			append(btnDefend);
		}
	}
}

