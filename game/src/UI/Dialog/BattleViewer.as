package src.UI.Dialog {

	import flash.display.DisplayObjectContainer;
	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.event.PopupEvent;
	import org.aswing.geom.*;
	import org.aswing.colorchooser.*;
	import org.aswing.ext.*;
	import src.Global;
	import src.Map.Username;
	import src.Objects.Factories.ObjectFactory;
	import src.UI.Components.CombatObjectGridList.CombatObjectGridList;
	import src.UI.GameJPanel;
	import src.Objects.Battle.*;
	import src.UI.LookAndFeel.GameLookAndFeel;
	import src.Util.StringHelper;

	public class BattleViewer extends GameJPanel {

		private var tabDefensive:JTabbedPane;
		private var pnlLog: JPanel;
		private var tabOffensive:JTabbedPane;
		private var lstLogScroll:JScrollPane;
		private var lblStamina: JLabel;

		private var battle: BattleManager;
		private var battleCityId: int;

		private var combat: Array = new Array();

		public function BattleViewer(battleCityId: int) {
			createUI();

			title = "Battle Viewer";

			this.battleCityId = battleCityId;

			battle = Global.mapComm.Battle.battleSubscribe(battleCityId);
			battle.addEventListener(BattleManager.OBJECT_ADDED_ATTACK, onAddedAttack);
			battle.addEventListener(BattleManager.OBJECT_REMOVED_ATTACK, onRemoved);
			battle.addEventListener(BattleManager.OBJECT_ADDED_DEFENSE, onAddedDefense);
			battle.addEventListener(BattleManager.OBJECT_REMOVED_DEFENSE, onRemoved);
			battle.addEventListener(BattleManager.OBJECT_ATTACKED, onAttack);
			battle.addEventListener(BattleManager.OBJECT_SKIPPED, onSkipped);
			battle.addEventListener(BattleManager.END, onEnd);
			battle.addEventListener(BattleManager.NEW_ROUND, onNewRound);

			// Every command sends the stamina so we update it with every action.
			battle.addEventListener(BattleManager.OBJECT_ADDED_ATTACK, updateStamina);
			battle.addEventListener(BattleManager.OBJECT_REMOVED_ATTACK, updateStamina);
			battle.addEventListener(BattleManager.OBJECT_ADDED_DEFENSE, updateStamina);
			battle.addEventListener(BattleManager.OBJECT_REMOVED_DEFENSE, updateStamina);
			battle.addEventListener(BattleManager.OBJECT_ATTACKED, updateStamina);
			battle.addEventListener(BattleManager.OBJECT_SKIPPED, updateStamina);
			battle.addEventListener(BattleManager.END, updateStamina);
		}

		public function onClosed(e: *):void
		{
			if (battle)
			{
				battle.removeEventListener(BattleManager.OBJECT_ADDED_ATTACK, onAddedAttack);
				battle.removeEventListener(BattleManager.OBJECT_REMOVED_ATTACK, onRemoved);
				battle.removeEventListener(BattleManager.OBJECT_ADDED_DEFENSE, onAddedDefense);
				battle.removeEventListener(BattleManager.OBJECT_REMOVED_DEFENSE, onRemoved);
				battle.removeEventListener(BattleManager.OBJECT_ATTACKED, onAttack);
				battle.removeEventListener(BattleManager.OBJECT_SKIPPED, onSkipped);
				battle.removeEventListener(BattleManager.END, onEnd);
				battle.removeEventListener(BattleManager.NEW_ROUND, onNewRound);

				battle.removeEventListener(BattleManager.OBJECT_ADDED_ATTACK, updateStamina);
				battle.removeEventListener(BattleManager.OBJECT_REMOVED_ATTACK, updateStamina);
				battle.removeEventListener(BattleManager.OBJECT_ADDED_DEFENSE, updateStamina);
				battle.removeEventListener(BattleManager.OBJECT_REMOVED_DEFENSE, updateStamina);
				battle.removeEventListener(BattleManager.OBJECT_ATTACKED, updateStamina);
				battle.removeEventListener(BattleManager.OBJECT_SKIPPED, updateStamina);
				battle.removeEventListener(BattleManager.END, updateStamina);

				Global.mapComm.Battle.battleUnsubscribe(battleCityId);
			}
		}

		private function onNewRound(e: BattleRoundEvent = null) : void {
			log(new JSeparator());
			logStr("Round " + e.round, null, true);
		}

		private function updateStamina(e: BattleEvent = null) : void {
			lblStamina.setText(StringHelper.makePlural(battle.attackers.size(), "Attacker", "Attackers") + " will run out of stamina in " + battle.stamina + " " + StringHelper.makePlural(battle.stamina, "round", "rounds") + ".");
		}

		private function addTab(combatObj: CombatObject, defense: Boolean) : Object {

			var obj: Object = findTab(combatObj.cityId, combatObj.troopStubId);
			if (obj != null) {
				(obj.grid as CombatObjectGridList).addCombatObject(combatObj);
				return obj;
			}

			var grid: CombatObjectGridList = CombatObjectGridList.getGridList([combatObj]);
			var tab: JScrollPane = new JScrollPane(grid);

			obj = { "cityId": combatObj.cityId, "troopStubId": combatObj.troopStubId, "defense": defense, "tab" : tab, "grid": grid };

			combat.push(obj);

			//tab.append(grid);

			if (defense)
			tabDefensive.appendTab(tab, combatObj.cityId + "(" + combatObj.troopStubId + ")");
			else
			tabOffensive.appendTab(tab, combatObj.cityId + "(" + combatObj.troopStubId + ")");

			Global.map.usernames.cities.getUsername(combatObj.cityId, setTabUsername, combatObj.troopStubId);

			return obj;
		}

		private function setTabUsername(username: Username, custom: *) : void
		{
			var idx: int = tabDefensive.indexOfTitle(username.id + "(" + custom + ")");
			if (idx > -1) tabDefensive.setTitleAt(idx, username.name + "(" + custom + ")");

			idx = tabOffensive.indexOfTitle(username.id + "(" + custom + ")");
			if (idx > -1) tabOffensive.setTitleAt(idx, username.name + "(" + custom + ")");
		}

		private function removeTab(cityId: int, troopStubId: int) : void {
			for (var i: int = 0; i < combat.length; i++)
			{
				var iteObj: Object = combat[i];

				if (cityId != iteObj.cityId || troopStubId != iteObj.troopStubId) continue;

				if (iteObj.defense)
				tabDefensive.remove(iteObj.tab);
				else
				tabOffensive.remove(iteObj.tab);

				combat.splice(i, 1);
				return;
			}
		}

		private function findTab(cityId: int, troopStubId: int) : Object {
			for each (var obj: Object in combat) {
				if (obj.cityId == cityId && obj.troopStubId == troopStubId)
				return obj;
			}

			return null;
		}

		private function findObject(list: CombatObjectGridList, combatObjectId: int) : * {
			var listData: VectorListModel = list.getModel() as VectorListModel;
			for (var i: int = 0; i < listData.size(); i++) {
				var combatObj: * = listData.getElementAt(i);
				if (combatObj.data.combatObjectId == combatObjectId) {
					return combatObj;
				}
			}

			return null;
		}

		public function onEnd(e: BattleEvent):void
		{
			tabOffensive.removeAll();
			tabDefensive.removeAll();

			logStr("Battle has ended");
		}

		public function onAddedAttack(e: BattleEvent):void
		{
			addTab(e.combatObj, false);
		}

		public function onRemoved(e: BattleEvent):void
		{
			var srcObj: Object = findTab(e.combatObj.cityId, e.combatObj.troopStubId);

			var attackObj: CombatObject = null;
			var listData: VectorListModel = (srcObj.grid as CombatObjectGridList).getModel() as VectorListModel;
			for (var i: int = 0; i < listData.size(); i++) {
				var combatObj: Object = listData.getElementAt(i);
				if (combatObj.data.combatObjectId == e.combatObj.combatObjectId) {
					listData.removeAt(i);
					break;
				}
			}

			if (listData.size() == 0) {
				removeTab(e.combatObj.cityId, e.combatObj.troopStubId);
			}
		}

		public function onAddedDefense(e: BattleEvent):void
		{
			addTab(e.combatObj, true);
		}

		public function onSkipped(e: BattleEvent) : void {
			var srcObj: Object = findTab(e.combatObj.cityId, e.combatObj.troopStubId);

			if (srcObj == null) {
				trace("Received skip for unknown object");
				return;
			}

			var attackObj: * = findObject(srcObj.grid, e.combatObj.combatObjectId);

			if (attackObj == null) {
				trace("Could not find attacker combat object");
				return;
			}

			var srcCityName: Username = Global.map.usernames.cities.getUsername(e.combatObj.cityId);

			var pnl: JPanel = new JPanel(new FlowLayout(AsWingConstants.CENTER, 0, 0, false));
			pnl.append(getCombatObjectPanel(srcCityName, e.combatObj));
			var lbl: JLabel = new JLabel("couldn't reach anyone", new AssetIcon(srcObj.defense ? new ICON_SHIELD : new ICON_SINGLE_SWORD));
			lbl.setHorizontalTextPosition(AsWingConstants.LEFT);
			pnl.append(lbl);

			log(pnl);
		}

		public function onAttack(e: BattleEvent):void
		{
			var destObj: Object = findTab(e.destCombatObj.cityId, e.destCombatObj.troopStubId);
			var srcObj: Object = findTab(e.combatObj.cityId, e.combatObj.troopStubId);

			if (srcObj == null || destObj == null) {
				trace("Received attack for unknown object");
				return;
			}

			//find attacker
			var attackObj: * = findObject(srcObj.grid, e.combatObj.combatObjectId);

			if (attackObj == null) {
				trace("Could not find attacker combat object");
				return;
			}

			//find defender
			var defenseObj: * = findObject(destObj.grid, e.destCombatObj.combatObjectId);

			if (defenseObj == null) {
				trace("Could not find defender combat object");
				return;
			}

			var srcCityName: Username = Global.map.usernames.cities.getUsername(e.combatObj.cityId);
			var destCityName: Username = Global.map.usernames.cities.getUsername(e.destCombatObj.cityId);

			var dmgPnl: JPanel = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 0, AsWingConstants.CENTER));
			dmgPnl.append(new JLabel(e.dmg.toString() + " dmg", new AssetIcon(!srcObj.defense ? new ICON_SHIELD : new ICON_SINGLE_SWORD)));
			var arrowPnl: JPanel = new JPanel(new FlowLayout(AsWingConstants.CENTER));
			arrowPnl.append(new AssetPane(srcObj.defense ? new ICON_ARROW_LEFT : new ICON_ARROW_RIGHT));
			dmgPnl.append(arrowPnl);

			var pnl: JPanel = new JPanel(new FlowLayout(AsWingConstants.CENTER, 7, 0, false));
			if (srcObj.defense) {
				pnl.append(getCombatObjectPanel(srcCityName, e.combatObj));
				pnl.append(dmgPnl);
				pnl.append(getCombatObjectPanel(destCityName, e.destCombatObj));
			} else {
				pnl.append(getCombatObjectPanel(destCityName, e.destCombatObj));
				pnl.append(dmgPnl);
				pnl.append(getCombatObjectPanel(srcCityName, e.combatObj));
			}

			log(pnl);

			if (defenseObj.data.hp <= 0) {
				pnl = new JPanel(new FlowLayout(AsWingConstants.CENTER, 0, 0, false));
				pnl.append(getCombatObjectPanel(srcCityName, e.combatObj));
				var defeatLbl: JLabel = new JLabel("has been defeated", new AssetIcon(!srcObj.defense ? new ICON_SHIELD : new ICON_SINGLE_SWORD));
				defeatLbl.setHorizontalTextPosition(AsWingConstants.LEFT);
				pnl.append(defeatLbl);
				log(pnl);
			}

			destObj.grid.getModel().valueChanged(defenseObj);
		}

		private function getCombatObjectPanel(cityName: Username, combatObj: CombatObject) : Component {
			var name: String = "";
			if (cityName != null) name += cityName.name;
			name += "(" + combatObj.troopStubId + ")'s " + combatObj.name;

			var icon: DisplayObjectContainer = ObjectFactory.getSpriteEx(combatObj.type, combatObj.level, true);
			if (ObjectFactory.getClassType(combatObj.type) == ObjectFactory.TYPE_STRUCTURE) icon = ObjectFactory.makeSpriteSmall(icon);

			var lbl: JLabel = new JLabel(name, (icon != null ? new AssetIcon(icon) : null));

			return lbl;
		}

		private function logStr(string: String, icon: Icon = null, header: Boolean = false) : void {
			var txt: JLabel = new JLabel(string, icon);
			if (header) GameLookAndFeel.changeClass(txt, "darkHeader");
			log(txt);
		}

		private function log(item: Component) : void {
			pnlLog.insert(0, item);

			if (pnlLog.getComponentCount() > 100) {
				pnlLog.removeAt(pnlLog.getComponentCount() - 1);
			}

			pnlLog.pack();
		}

		public function show(owner:* = null, modal:Boolean = true, onClose:Function = null):JFrame
		{
			super.showSelf(owner, modal, onClose);

			frame.addEventListener(PopupEvent.POPUP_CLOSED, onClosed);

			Global.gameContainer.showFrame(frame);
			return frame;
		}

		private function createUI(): void {
			var layout0:SoftBoxLayout = new SoftBoxLayout();
			layout0.setAxis(AsWingConstants.VERTICAL);
			layout0.setGap(5);
			setLayout(layout0);
			setBorder(null);
			setPreferredSize(new IntDimension(650, 600));

			var pnlBody: JPanel = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 5));
			pnlBody.setBorder(null);

			lblStamina = new JLabel("", null, AsWingConstants.RIGHT);

			tabDefensive = new JTabbedPane();
			tabDefensive.setPreferredHeight(175);

			var border1:SimpleTitledBorder = new SimpleTitledBorder(null, "Defender", AsWingConstants.TOP, AsWingConstants.LEFT);
			tabDefensive.setBorder(border1);

			pnlLog = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 8, AsWingConstants.CENTER));
			pnlLog.setBorder(new EmptyBorder(null, new Insets(10, 0, 5, 0)));

			lstLogScroll = new JScrollPane(pnlLog);
			lstLogScroll.setBorder(new EmptyBorder(null, new Insets()));

			var tabLog: JTabbedPane = new JTabbedPane();
			tabLog.appendTab(lstLogScroll, "Battle Log");
			tabLog.setPreferredHeight(150);

			tabOffensive = new JTabbedPane();
			tabOffensive.setPreferredHeight(175);

			var border3: SimpleTitledBorder = new SimpleTitledBorder(null, "Attacker", AsWingConstants.TOP, AsWingConstants.LEFT);
			tabOffensive.setBorder(border3);

			//component layoution
			pnlBody.append(tabDefensive);
			pnlBody.append(tabLog);
			pnlBody.append(tabOffensive);

			append(lblStamina);
			append(pnlBody);
		}
	}

}

