
package src.UI.Sidebars.ObjectInfo {

	import flash.display.*;
	import flash.events.*;
	import flash.geom.Point;
	import flash.text.*;
	import flash.utils.Timer;
	import src.Constants;
	import src.Global;
	import src.Map.*;
	import src.Objects.Actions.*;
	import src.Objects.Factories.*;
	import src.Objects.*;
	import src.Objects.Prototypes.*;
	import src.UI.*;
	import src.UI.Components.GoToCityIcon;
	import src.UI.Components.Messaging.MessagingIcon;
	import src.UI.Sidebars.ObjectInfo.Buttons.*;
	import src.Util.BinaryList.*;
	import src.Util.Util;

	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.geom.*;
	import org.aswing.colorchooser.*;
	import org.aswing.ext.*;

	public class ObjectInfoSidebar extends GameJSidebar
	{
		//UI
		private var lblName:JLabel;
		private var pnlStats:Form;
		private var pnlUpgrades:JPanel;
		private var pnlGroups:JPanel;
		private var pnlActions:JPanel;

		private var gameObject: StructureObject;

		private var buttons: Array = new Array();
		private var t:Timer = new Timer(1000);

		public function ObjectInfoSidebar(obj: StructureObject)
		{
			this.gameObject = obj;

			var city: City = Global.map.cities.get(gameObject.cityId);

			if (city != null)
			{
				city.addEventListener(City.RESOURCES_UPDATE, onResourcesUpdate);
				city.currentActions.addEventListener(BinaryListEvent.CHANGED, onObjectUpdate);
			}

			gameObject.addEventListener(SimpleGameObject.OBJECT_UPDATE, onObjectUpdate);
			gameObject.actionReferences.addEventListener(BinaryListEvent.CHANGED, onObjectUpdate);

			createUI();
			update();
		}

		private function setPlayerUsername(username: Username, custom: * ) : void {
			var usernameLabel: JLabel = custom as JLabel;

			usernameLabel.setText(username.name);

			// Show message icon if its not the current player
			if (username.id != Constants.playerId) {
				usernameLabel.setIcon(new MessagingIcon(username.name));
				usernameLabel.setHorizontalTextPosition(AsWingConstants.LEFT);
			}
		}

		private function setCityUsername(username: Username, custom: * ) : void {
			var usernameLabel: JLabel = custom as JLabel;

			usernameLabel.setText(username.name);

			if (username.id != Global.gameContainer.selectedCity.id) {
				usernameLabel.setIcon(new GoToCityIcon(username.id));
				usernameLabel.setHorizontalTextPosition(AsWingConstants.LEFT);
			}
		}

		public function update():void
		{
			t.reset();

			clear();

			buttons = new Array();

			var structPrototype: StructurePrototype = StructureFactory.getPrototype(gameObject.type, gameObject.level);

			if (!structPrototype) return;

			var structureObject: StructureObject = gameObject as StructureObject;

			var usernameLabel: JLabel = addStatRow("Player", "-");
			var cityLabel: JLabel = addStatRow("City", "-");

			Global.map.usernames.players.getUsername(gameObject.playerId, setPlayerUsername, usernameLabel);
			Global.map.usernames.cities.getUsername(gameObject.cityId, setCityUsername, cityLabel);

			addStatRow("Level", gameObject.level.toString());

			var city: City = Global.map.cities.get(gameObject.cityId);

			//We check anywhere for city != null to make sure it belongs to this player
			//since we only display basic stats for non owner viewing this building
			if (city != null) {
				// Only show stats if obj is attackable
				if (!ObjectFactory.isType("Unattackable", structPrototype.type)) {
					addStatRow("HP", gameObject.hp.toString() + "/" + structPrototype.hp.toString());
					addStatRow("Attack", structPrototype.defense.toString());
					addStatRow("Range", structPrototype.range.toString());
					addStatRow("Stealth", structPrototype.stealth.toString());
				}

				if (structPrototype.maxlabor > 0) {
					addStatRow("Laborers", gameObject.labor + "/" + structPrototype.maxlabor, new AssetIcon(new ICON_LABOR()));
				} else if (gameObject.labor > 0) {
					addStatRow("Laborers", gameObject.labor.toString(), new AssetIcon(new ICON_LABOR()));
				}

				var propPrototype: Array = PropertyFactory.getAllProperties(gameObject.type);

				if (structureObject != null)
				{
					for (var i: int = 0; i < propPrototype.length; i++)
					addStatRow(propPrototype[i].name, structureObject.properties[i]);

					buttons = buttons.concat(StructureFactory.getButtons(structureObject)).concat(StructureFactory.getTechButtons(structureObject));
				}
			}
			else {
				propPrototype = PropertyFactory.getProperties(gameObject.type, PropertyPrototype.VISIBILITY_PUBLIC);

				if (structureObject != null)
				{
					for (i = 0; i < structureObject.properties.length; i++) {
						addStatRow(propPrototype[i].name, structureObject.properties[i]);
					}
				}
			}

			//Special Case Buttons
			switch(structureObject.State.getStateType())
			{
				case SimpleGameObject.STATE_BATTLE:
					buttons.push(new ViewBattleButton(structureObject));
				break;
				case SimpleGameObject.STATE_MOVING:
					buttons.push(new ViewDestinationButton(structureObject));
				break;					
			}

			var buttonsCache: Array = buttons.concat();
			for each(var group: Object in Action.groups) {
				var groupedButtons: Array = new Array();
				for each (var type: * in group.actions) {
					var tmp: Array = new Array();
					for (i = buttonsCache.length - 1; i >= 0; i--) {
						var button: ActionButton = buttonsCache[i];
						if (!(button is type)) continue;
						tmp.push(button);
						buttonsCache.splice(i, 1);
					}

					tmp.sort(function(a:ActionButton, b:ActionButton):Number {
						var aIndex: Number = (a.parentAction ? a.parentAction.index : 0);
						var bIndex: Number = (b.parentAction ? b.parentAction.index : 0);

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
				}

				pnlGroups.append(pnlGroup);
			}

			if (city == null) return;

			validateButtons();
			displayCurrentActions();

			t.addEventListener(TimerEvent.TIMER, onUpdateTimer);
			t.start();
		}

		private function addStatRow(title: String, value: String, icon: Icon = null) : JLabel {
			var rowTitle: JLabel = new JLabel(title);
			rowTitle.setHorizontalAlignment(AsWingConstants.LEFT);
			rowTitle.setName("title");

			var rowValue: JLabel = new JLabel(value);
			rowValue.setHorizontalAlignment(AsWingConstants.LEFT);
			rowValue.setHorizontalTextPosition(AsWingConstants.LEFT);
			rowValue.setName("value");
			rowValue.setIcon(icon);

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

			var city: City = Global.map.cities.get(gameObject.cityId);

			if (city == null) return;

			var actions: Array = city.currentActions.getObjectActions(gameObject.objectId);
			for each (var actionReference: CurrentActionReference in gameObject.actionReferences.each())
			actions.push(actionReference);

			for (var i: int = 0; i < actions.length; i++)
			{
				var currentAction: * = actions[i];

				var actionDescription: String = currentAction.toString(gameObject);

				if (currentAction is CurrentActionReference) {
					currentAction = currentAction.getAction(gameObject);
				}

				var cancelButton: CancelActionButton = new CancelActionButton(gameObject, currentAction.id);

				var timeLeft: int = Math.max(0, currentAction.endTime - Global.map.getServerTime());

				//component creation
				var pnlActionRow: JPanel = new JPanel(new BorderLayout());

				var panel: JPanel = new JPanel();
				panel.setConstraints("North");
				panel.setLayout(new BorderLayout());

				var lblDescription: MultilineLabel = new MultilineLabel(actionDescription);
				lblDescription.setConstraints("West");
				lblDescription.mouseEnabled = false;

				var astCancel: AssetPane = new AssetPane(cancelButton);
				astCancel.setConstraints("East");

				var lblTime: JLabel = new JLabel(Util.formatTime(timeLeft));
				lblTime.setHorizontalAlignment(AsWingConstants.RIGHT);
				lblTime.setIcon(new AssetIcon(new ICON_CLOCK()));
				lblTime.setConstraints("South");

				//component layoution
				panel.append(lblDescription);
				panel.append(astCancel);

				pnlActionRow.append(panel);
				pnlActionRow.append(lblTime);

				pnlActions.append(pnlActionRow);
			}
		}

		public function dispose():void
		{
			t.stop();
			t = null;

			if (gameObject != null)
			{
				var city: City = Global.map.cities.get(gameObject.cityId);

				if (city != null)
				{
					city.removeEventListener(City.RESOURCES_UPDATE, onResourcesUpdate);
					city.currentActions.removeEventListener(BinaryListEvent.CHANGED, onObjectUpdate);
				}

				gameObject.removeEventListener(SimpleGameObject.OBJECT_UPDATE, onObjectUpdate);
				gameObject.actionReferences.removeEventListener(BinaryListEvent.CHANGED, onObjectUpdate);

				gameObject.actionReferences.clear();
			}
		}

		public function onResourcesUpdate(event: Event):void
		{
			validateButtons();
		}

		public function onObjectUpdate(event: Event):void
		{
			update();

			if (getFrame() != null)
			getFrame().pack();
		}

		public function validateButtons():void
		{
			var structPrototype: StructurePrototype = StructureFactory.getPrototype(gameObject.type, gameObject.level);
			var workerPrototype: Worker = null;

			if (structPrototype)
			workerPrototype = WorkerFactory.getPrototype(structPrototype.workerid);

			var city: City = Global.map.cities.get(gameObject.cityId);

			for each(var button: ActionButton in buttons)
			{
				button.validateButton();

				if (!button.countCurrentActions() || (city != null && workerPrototype != null && city.currentActions.getObjectActions(gameObject.objectId, true).length >= workerPrototype.maxCount)) {
					button.disable();
				}
			}
		}

		private function createUI() : void
		{
			//component creation
			//setPreferredHeight(GameJSidebar.FULL_HEIGHT);
			setLayout(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 5));

			lblName = new JLabel();
			lblName.setFont(new ASFont("Tahoma", 11, true, false, false, false));
			lblName.setText("Name (x,y)");
			lblName.setHorizontalAlignment(AsWingConstants.LEFT);

			pnlStats = new Form();

			pnlGroups = new JPanel();
			pnlGroups.setLayout(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 5));
			pnlGroups.setBorder(new EmptyBorder(null, new Insets(0, 0, 20, 0)));

			pnlActions = new JPanel();
			pnlActions.setLayout(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 10));

			//component layoution
			//append(lblName);
			append(pnlStats);
			append(pnlGroups);
			append(pnlActions);
		}

		override public function show(owner:* = null, onClose:Function = null):JFrame
		{
			super.showSelf(owner, onClose, dispose);

			var structPrototype: StructurePrototype = StructureFactory.getPrototype(gameObject.type, gameObject.level);
			if (structPrototype) {
				var pt: Point = MapUtil.getMapCoord(gameObject.getX(), gameObject.getY());
				frame.getTitleBar().setText(structPrototype.getName() + " (" + pt.x + "," + pt.y + ")");
			}

			frame.show();
			return frame;
		}
	}

}

