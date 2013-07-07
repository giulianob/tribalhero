package src.UI.Sidebars.TroopInfo {
	import flash.events.*;
	import flash.geom.Point;
	import src.Map.*;
	import flash.text.*;
	import src.Objects.Factories.*;
	import src.Objects.Prototypes.*;
	import src.Objects.*;
	import src.UI.Components.CityLabel;
	import src.UI.Components.GoToCityIcon;
	import src.UI.Components.Messaging.MessagingIcon;
	import src.UI.Components.PlayerLabel;
	import src.UI.Dialog.*;
	import src.UI.GameJSidebar;
	import src.UI.Sidebars.ObjectInfo.Buttons.*;
	import src.Util.*;
	import flash.display.*;
	import flash.utils.Timer;
	import src.Objects.Actions.*;
	import src.*;
	import src.Objects.Troop.*;

	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.geom.*;
	import org.aswing.colorchooser.*;
	import org.aswing.ext.*;

	public class TroopInfoSidebar extends GameJSidebar
	{
		//UI
		private var lblName:JLabel;
		private var pnlStats:Form;
		private var pnlUpgrades:JPanel;
		private var pnlGroups:JPanel;
		private var pnlActions:JPanel;

		private var troopObj: TroopObject;

		private var t:Timer = new Timer(1000);

		public function TroopInfoSidebar(troopObj: TroopObject)
		{
			this.troopObj = troopObj;

			troopObj.addEventListener(SimpleGameObject.OBJECT_UPDATE, onObjectUpdate);

			createUI();
			update();
		}

		public function onObjectUpdate(e: Event):void
		{
			update();
		}

		public function update():void
		{
			t.reset();

			clear();

			var usernameLabel: PlayerLabel = addStatRow("Player", new PlayerLabel(troopObj.playerId));
			
			var cityLabel: CityLabel = addStatRow("City", new CityLabel(troopObj.cityId));

			var buttons: Array = [];

			var city: City = Global.map.cities.get(troopObj.cityId);

			if (city != null) {
				addStatRow("Radius", troopObj.attackRadius.toString());
				addStatRow("Speed", troopObj.speed.toFixed(1));
				
				buttons.push(new ViewDestinationButton(troopObj, new Point(troopObj.targetX, troopObj.targetY)));
			}

			//Special Case Buttons
			switch(troopObj.state.getStateType())
			{
				case SimpleGameObject.STATE_BATTLE:
					buttons.push(new ViewBattleButton(troopObj));
				break;
			}		

			//Add buttons to UI
			for each(var group: Object in Action.groups) {
				var groupedButtons: Array = [];
				for each (var type: * in group.actions) {
					var tmp: Array = [];
					for (var i: int = buttons.length - 1; i >= 0; i--) {
						var button: ActionButton = buttons[i];
						if (!(button is type)) continue;
						tmp.push(button);
						buttons.splice(i, 1);
					}

					tmp.sort(function(a:ActionButton, b:ActionButton):Number {
						var aIndex: Number = a.parentAction.index;
						var bIndex: Number = b.parentAction.index;

						if (aIndex > bIndex)
						return 1;
						else if (aIndex < bIndex)
						return -1;
						else
						return 0;
					}
					);

					groupedButtons = groupedButtons.concat(tmp);
				}

				if (groupedButtons.length == 0) continue;				
				
				var pnlGroup: JPanel = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 3));
				pnlGroup.setBorder(new TitledBorder(null, group.name, AsWingConstants.TOP, AsWingConstants.CENTER, 0, 10));
				
				for each(var groupButton: ActionButton in groupedButtons) {
					if (groupButton.parentAction == null) continue;

					pnlGroup.append(groupButton);
					
					if (!groupButton.validateButton())
						groupButton.disable();
				}

				pnlGroups.append(pnlGroup);
			}

			if (city == null)
			return;

			displayCurrentActions();

			t.addEventListener(TimerEvent.TIMER, onUpdateTimer);
			t.start();
		}

		private function addStatRow(title: String, textOrComponent: *, icon: Icon = null) : * {
			var rowTitle: JLabel = new JLabel(title);
			rowTitle.setHorizontalAlignment(AsWingConstants.LEFT);
			rowTitle.setName("title");

			var rowValue: Component;
			if (textOrComponent is String) {
				var label: JLabel = new JLabel(textOrComponent as String);
				label.setHorizontalAlignment(AsWingConstants.LEFT);
				label.setHorizontalTextPosition(AsWingConstants.LEFT);
				label.setName("value");
				label.setIcon(icon);
				rowValue = label;
			} 
			else			
				rowValue = textOrComponent as Component;			

			pnlStats.addRow(rowTitle, rowValue);

			return rowValue;
		}

		private function clear():void
		{
			pnlGroups.removeAll();
			pnlStats.removeAll();
		}

		public function onUpdateTimer(event: TimerEvent):void
		{
			displayCurrentActions();
		}

		public function displayCurrentActions():void
		{
			pnlActions.removeAll();

			var city: City = Global.map.cities.get(troopObj.cityId);

			if (city == null) return;

			var actions: Array = city.currentActions.getObjectActions(troopObj.objectId);
			for each (var actionReference: CurrentActionReference in city.references.getByObject(troopObj.objectId)) {
				actions.push(actionReference);
			}

			for (var i: int = 0; i < actions.length; i++)
			{
				var currentAction: * = actions[i];

				var actionDescription: String = currentAction.toString();

				if (currentAction is CurrentActionReference) {					
					currentAction = currentAction.getAction();
					if (!currentAction) continue;
				}

				var cancelButton: CancelActionButton = new CancelActionButton(troopObj, currentAction.id);

				var timeLeft: int = currentAction.endTime > 0 ? currentAction.endTime - Global.map.getServerTime() : 0;

				var finishedAction: Boolean = false;
				var color: String = "0x000000";

				if (timeLeft < 0)
				{
					//continue;
					timeLeft = -timeLeft;
					finishedAction = true;
				}

				//component creation
				var pnlActionRow: JPanel = new JPanel(new BorderLayout());

				var panel: JPanel = new JPanel();
				panel.setConstraints("North");
				panel.setLayout(new BorderLayout());

				var lblDescription: JLabel = new JLabel(actionDescription);
				lblDescription.setConstraints("West");
				lblDescription.setHorizontalAlignment(AsWingConstants.LEFT);

				var astCancel: AssetPane = new AssetPane(cancelButton);
				astCancel.setConstraints("East");

				var lblTime: JLabel = new JLabel(Util.formatTime(timeLeft));
				lblTime.setHorizontalAlignment(AsWingConstants.RIGHT);
				lblTime.setIcon(new AssetIcon(new ICON_CLOCK()));
				lblTime.setConstraints("South");

				//component layoution
				panel.append(lblDescription);

				if (currentAction.isCancellable())
					panel.append(astCancel);

				pnlActionRow.append(panel);
				pnlActionRow.append(lblTime);

				pnlActions.append(pnlActionRow);
			}
		}

		public function dispose():void
		{
			troopObj.removeEventListener(SimpleGameObject.OBJECT_UPDATE, onObjectUpdate);

			t.stop();
			t = null;
		}

		private function createUI() : void
		{
			//component creation
			setSize(new IntDimension(288, 180));
			setLayout(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 5));

			lblName = new JLabel();
			lblName.setFont(new ASFont("Tahoma", 11, true, false, false, false));
			lblName.setSize(new IntDimension(400, 17));
			lblName.setText("Name (x,y)");
			lblName.setHorizontalAlignment(AsWingConstants.LEFT);

			pnlStats = new Form();

			pnlGroups = new JPanel();
			pnlGroups.setLayout(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 3));
			pnlGroups.setBorder(new EmptyBorder(null, new Insets(0, 0, 20, 0)));

			pnlActions = new JPanel();
			pnlActions.setLayout(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 10));
			pnlActions.setSize(new IntDimension(288, 10));

			//component layoution
			//append(lblName);
			append(pnlStats);
			append(pnlGroups);
			append(pnlActions);
		}

		override public function show(owner:* = null, onClose:Function = null):JFrame
		{
			super.showSelf(owner, onClose, dispose);

			var pt: Point = TileLocator.getMapCoord(troopObj.objX, troopObj.objY);
			frame.getTitleBar().setText("Troop (" + pt.x + "," + pt.y + ")");

			frame.show();
			return frame;
		}
	}

}

