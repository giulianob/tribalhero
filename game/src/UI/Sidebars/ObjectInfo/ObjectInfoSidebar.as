
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
	import src.Objects.Process.AttackSendProcess;
	import src.Objects.Prototypes.*;
	import src.UI.*;
	import src.UI.Components.CityLabel;
	import src.UI.Components.GoToCityIcon;
	import src.UI.Components.Messaging.MessagingIcon;
	import src.UI.Components.PlayerLabel;
	import src.UI.Components.SimpleTooltip;
	import src.UI.Components.StarRating;
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
		private var scrollGroups: JScrollPane;

		public var gameObject: StructureObject;

		public var buttons: Array = new Array();
		private var t:Timer = new Timer(1000);

		public function ObjectInfoSidebar(obj: StructureObject)
		{
			this.gameObject = obj;

			var city: City = Global.map.cities.get(gameObject.cityId);

			if (city != null)
			{
				city.addEventListener(City.RESOURCES_UPDATE, onResourcesUpdate);
				city.currentActions.addEventListener(BinaryListEvent.CHANGED, onObjectUpdate);
				city.references.addEventListener(BinaryListEvent.CHANGED, onObjectUpdate);
			}

			gameObject.addEventListener(SimpleGameObject.OBJECT_UPDATE, onObjectUpdate);

			createUI();
			
			addEventListener(Event.ADDED_TO_STAGE, function(e: Event): void {
				update();
			});			
			
			Global.gameContainer.resizeManager.addEventListener(Event.RESIZE, onObjectUpdate);
		}

		public function update():void
		{			
			t.reset();
			
			if (!getFrame())
				return;

			clear();

			buttons = new Array();

			var structPrototype: StructurePrototype = StructureFactory.getPrototype(gameObject.type, gameObject.level);

			if (!structPrototype) return;

			var structureObject: StructureObject = gameObject as StructureObject;

			var usernameLabel: PlayerLabel = addStatRow("Player", new PlayerLabel(gameObject.playerId));
			
			var cityLabel: CityLabel = addStatRow("City", new CityLabel(gameObject.cityId));

			addStatRow("Level", gameObject.level.toString());

			var city: City = Global.map.cities.get(gameObject.cityId);

			//We check anywhere for city != null to make sure it belongs to this player
			//since we only display basic stats for non owner viewing this building
			if (city != null) {
				// Only show stats if obj is attackable
				if (!ObjectFactory.isType("Unattackable", structPrototype.type)) {
					addStatRow("HP", gameObject.hp.toString() + "/" + structPrototype.hp.toString());
				}

				if (structPrototype.maxlabor > 0) {
					addStatRow("Laborers", gameObject.labor + "/" + structPrototype.maxlabor, new AssetIcon(new ICON_LABOR()));
				} else if (gameObject.labor > 0) {
					addStatRow("Laborers", gameObject.labor.toString(), new AssetIcon(new ICON_LABOR()));
				}

				var propPrototype: Array = PropertyFactory.getAllProperties(gameObject.type);

				if (structureObject != null)
				{
					for (var i: int = 0; i < propPrototype.length; i++) {
						var lbl: JLabel = addStatRow(propPrototype[i].name, propPrototype[i].toString(structureObject.properties[i]), propPrototype[i].getIcon());
						if (propPrototype[i].tooltip != "") new SimpleTooltip(lbl, propPrototype[i].tooltip);
					}

					buttons = buttons.concat(StructureFactory.getButtons(structureObject)).concat(StructureFactory.getTechButtons(structureObject));
				}
			}
			else {
				propPrototype = PropertyFactory.getProperties(gameObject.type, PropertyPrototype.VISIBILITY_PUBLIC);

				if (structureObject != null)
				{
					for (i = 0; i < propPrototype.length; i++) {
						lbl = addStatRow(propPrototype[i].name, propPrototype[i].toString(structureObject.properties[i]), propPrototype[i].getIcon());
						if (propPrototype[i].tooltip != "") new SimpleTooltip(lbl, propPrototype[i].tooltip);
					}
                    
                    if (!ObjectFactory.isType("Unattackable", gameObject.type)) {
                        buttons.push(new SendAttackButton(gameObject, new Location(Location.CITY, gameObject.groupId, gameObject.objectId))); 
                    }
				}
			}
			
			if (Global.gameContainer.selectedCity.id != gameObject.cityId) {
				buttons.push(new SendReinforcementButton(gameObject, new Location(Location.CITY, gameObject.groupId, gameObject.objectId)));
			}

			//Special Case Buttons
			switch(structureObject.state.getStateType())
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

				var pnlGroup: JPanel = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 3));
				pnlGroup.setBorder(new TitledBorder(null, group.name, AsWingConstants.TOP, AsWingConstants.CENTER, 0, 10));

				for each(var groupButton: ActionButton in groupedButtons) {
					if (groupButton.parentAction == null) continue;

					pnlGroup.append(groupButton);
				}

				pnlGroups.append(pnlGroup);
			}
		
			getFrame().pack();
			
			if (city != null) {
				validateButtons();
				displayCurrentActions();
				
				setPreferredHeight( -1);
				repaintAndRevalidate();								
				pack();
				
				setPreferredHeight(Math.min(getSize().height, Util.getMaxGamePanelHeight(getFrame().getGlobalLocation().y)));				

				pack();
				getFrame().pack();
				
				t.addEventListener(TimerEvent.TIMER, onUpdateTimer);
				t.start();
			}							
		}

		private function addStatStarRow(title: String, value: int, min: int, max: int) : void {
			var rowTitle: JLabel = new JLabel(title);
			rowTitle.setHorizontalAlignment(AsWingConstants.LEFT);
			rowTitle.setName("title");

			var rowValue: StarRating = new StarRating(min, max, value, 5);

			pnlStats.addRow(rowTitle, rowValue);
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

			var city: City = Global.map.cities.get(gameObject.cityId);

			if (city == null) return;

			var actions: Array = city.currentActions.getObjectActions(gameObject.objectId);
			for each (var actionReference: CurrentActionReference in city.references.getByObject(gameObject.objectId)) {
				actions.push(actionReference);
			}

			for (var i: int = 0; i < actions.length; i++)
			{
				var currentAction: * = actions[i];				

				if (currentAction is CurrentActionReference) {
					currentAction = currentAction.getAction();
					if (!currentAction) continue;
				}
				
				var actionDescription: String = currentAction.toString();

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
				
				if (currentAction.isCancellable())
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

			Global.gameContainer.resizeManager.removeEventListener(Event.RESIZE, onObjectUpdate);
			
			if (gameObject != null)
			{
				var city: City = Global.map.cities.get(gameObject.cityId);							

				if (city != null)
				{
					city.removeEventListener(City.RESOURCES_UPDATE, onResourcesUpdate);
					city.currentActions.removeEventListener(BinaryListEvent.CHANGED, onObjectUpdate);
					city.references.removeEventListener(BinaryListEvent.CHANGED, onObjectUpdate);
				}

				gameObject.removeEventListener(SimpleGameObject.OBJECT_UPDATE, onObjectUpdate);
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

			if (structPrototype) {
				workerPrototype = WorkerFactory.getPrototype(structPrototype.workerid);
			}

			var city: City = Global.map.cities.get(gameObject.cityId);

			for each(var button: ActionButton in buttons)
			{
				button.enable();
				
				if (button.alwaysEnabled() || Constants.alwaysEnableButtons) {					
					continue;
				}				
				
				if (!button.validateButton() || !city.validateAction(button.parentAction, gameObject))
					button.disable();
			}
		}

		private function createUI() : void
		{
			//component creation
			setLayout(new BorderLayout(0, 5));

			lblName = new JLabel();
			lblName.setFont(new ASFont("Tahoma", 11, true, false, false, false));
			lblName.setText("Name (x,y)");
			lblName.setHorizontalAlignment(AsWingConstants.LEFT);

			pnlStats = new Form();
			pnlStats.setConstraints("North");

			pnlGroups = new JPanel();
			pnlGroups.setLayout(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 5));
			pnlGroups.setBorder(new EmptyBorder(null, new Insets(0, 0, 20, 0)));		

			pnlActions = new JPanel();
			pnlActions.setLayout(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 10));
			pnlActions.setConstraints("South");

			var viewPort: JViewport = new JViewport(pnlGroups, true, false);
			viewPort.setVerticalAlignment(AsWingConstants.TOP);
			scrollGroups = new JScrollPane(viewPort, JScrollPane.SCROLLBAR_AS_NEEDED, JScrollPane.SCROLLBAR_NEVER);
			scrollGroups.setConstraints("Center");
			
			//component layoution
			append(pnlStats);
			append(scrollGroups);
			append(pnlActions);
		}

		override public function show(owner:* = null, onClose:Function = null):JFrame
		{
			super.showSelf(owner, onClose, dispose);

			var structPrototype: StructurePrototype = StructureFactory.getPrototype(gameObject.type, gameObject.level);
			if (structPrototype) {
				var pt: Point = MapUtil.getMapCoord(gameObject.objX, gameObject.objY);
				frame.getTitleBar().setText(structPrototype.getName() + " (" + pt.x + "," + pt.y + ")");
			}

			frame.show();
			return frame;
		}
	}

}

