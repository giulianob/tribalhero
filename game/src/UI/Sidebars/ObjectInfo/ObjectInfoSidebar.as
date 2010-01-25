
package src.UI.Sidebars.ObjectInfo {

	import flash.display.*;
	import flash.events.*;
	import flash.geom.Point;
	import flash.text.*;
	import flash.utils.Timer;
	import src.Global;
	import src.Map.*;
	import src.Objects.Actions.*;
	import src.Objects.Factories.*;
	import src.Objects.*;
	import src.Objects.Prototypes.*;
	import src.UI.*;
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

		public function update():void
		{
			t.reset();

			clear();
			
			buttons = new Array();

			var structPrototype: StructurePrototype = StructureFactory.getPrototype(gameObject.type, gameObject.level);

			if (!structPrototype) return;

			var structureObject: StructureObject = gameObject as StructureObject;

			var usernameLabel: JLabel = addStatRow("Owner", "-");
			var cityLabel: JLabel = addStatRow("City", "-");

			Global.map.usernames.players.setLabelUsername(gameObject.playerId, usernameLabel);
			Global.map.usernames.cities.setLabelUsername(gameObject.cityId, cityLabel);

			addStatRow("Level", gameObject.level.toString());

			var city: City = Global.map.cities.get(gameObject.cityId);

			//We check anywhere for city != null to make sure it belongs to this player
			//since we only display basic stats for non owner viewing this building
			if (city != null) {
				addStatRow("HP", gameObject.hp.toString() + "/" + structPrototype.hp.toString());
				addStatRow("Attack", structPrototype.defense.toString());
				addStatRow("Range", structPrototype.range.toString());
				addStatRow("Stealth", structPrototype.stealth.toString());
				if (structPrototype.maxlabor > 0)
				addStatRow("Labor", gameObject.labor + "/" + structPrototype.maxlabor);

				var propPrototype: Array = PropertyFactory.getProperties(gameObject.type);

				if (structureObject != null)
				{
					for (var i: int = 0; i < structureObject.properties.length; i++)
					addStatRow(propPrototype[i].name, structureObject.properties[i]);

					buttons = buttons.concat(StructureFactory.getButtons(structureObject)).concat(StructureFactory.getTechButtons(structureObject));
				}
			}

			//Special Case Buttons
			switch(structureObject.State)
			{
				case SimpleGameObject.STATE_BATTLE:
					buttons.push(new ViewBattleButton(structureObject));
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

				var pnlGroup: JPanel = new JPanel();
				var border: TitledBorder = new TitledBorder();
				pnlGroup.setLayout(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 0));
				border.setColor(new ASColor(0x0, 1));
				border.setTitle(group.name);
				border.setBeveled(false);
				border.setRound(10);
				pnlGroup.setBorder(border);

				var cnt: int = 0;
				var lastActionType: int = -1;
				var pnlRow: JPanel = null;
				for each(var groupButton: ActionButton in groupedButtons) {
					if (groupButton.parentAction == null) continue;

					if (cnt == 3 || lastActionType == -1 || lastActionType != groupButton.parentAction.actionType) {
						pnlRow = new JPanel();
						pnlRow.setLayout(new FlowLayout(AsWingConstants.LEFT, 8));
						pnlGroup.append(pnlRow);
						lastActionType = (groupButton.parentAction ? groupButton.parentAction.actionType : 0);
						cnt = 0;
					}

					pnlRow.append(new AssetPane(groupButton));

					cnt++;
				}

				pnlGroups.append(pnlGroup);
			}

			if (city == null) return;
			
			validateButtons();
			displayCurrentActions();

			t.addEventListener(TimerEvent.TIMER, onUpdateTimer);
			t.start();
		}

		private function addStatRow(title: String, value: String) : JLabel {
			var rowTitle: JLabel = new JLabel(title);
			rowTitle.setHorizontalAlignment(AsWingConstants.LEFT);
			rowTitle.setName("title");

			var rowValue: JLabel = new JLabel(value);
			rowValue.setHorizontalAlignment(AsWingConstants.LEFT);
			rowValue.setName("value");

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

				if (currentAction is CurrentActionReference)
				currentAction = currentAction.getAction(gameObject);

				var cancelButton: CancelActionButton = new CancelActionButton(gameObject, currentAction.id);

				var timeLeft: int = Math.max(0, currentAction.endTime - Global.map.getServerTime());

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

				if (gameObject is StructureObject)
				(gameObject as StructureObject).clearProperties();

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

				if (!button.countCurrentActions() && (city != null && workerPrototype != null && city.currentActions.getObjectActions(gameObject.objectId).length >= workerPrototype.maxCount))
				button.disable();
			}
		}

		private function createUI() : void
		{
			//component creation
			setPreferredHeight(GameJSidebar.FULL_HEIGHT);
			setLayout(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 5));

			lblName = new JLabel();
			lblName.setFont(new ASFont("Tahoma", 11, true, false, false, false));
			lblName.setText("Name (x,y)");
			lblName.setHorizontalAlignment(AsWingConstants.LEFT);

			pnlStats = new Form();

			pnlGroups = new JPanel();
			pnlGroups.setLayout(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 3));
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

