package src.UI.Dialog {

	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.event.PopupEvent;
	import org.aswing.geom.*;
	import org.aswing.colorchooser.*;
	import org.aswing.ext.*;
	import src.Global;
	import src.Map.Username;
	import src.UI.Components.CombatObjectGridList.CombatObjectGridList;
	import src.UI.GameJPanel;
	import src.Objects.Battle.*;
	import src.Util.StringHelper;

	public class BattleViewer extends GameJPanel {

		private var tabDefensive:JTabbedPane;
		private var txtLog: JTextArea;
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
			var tab: JPanel = new JPanel(new BorderLayout());

			obj = { "cityId": combatObj.cityId, "troopStubId": combatObj.troopStubId, "defense": defense, "tab" : tab, "grid": grid };

			combat.push(obj);

			tab.append(grid);

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

			log("Battle has ended");

			getFrame().dispose();
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

			log((srcCityName != null ? srcCityName.name + "(" + e.combatObj.troopStubId  + ")'s " : "") + attackObj.data.name + " couldn't hit anyone.");
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

			log((srcCityName != null ? srcCityName.name + "(" + e.combatObj.troopStubId  + ")'s " : "") + attackObj.data.name + " hit " + (destCityName != null ? destCityName.name + "(" + e.destCombatObj.troopStubId  + ")'s " : "") + defenseObj.data.name + " for " + e.dmg + " dmg");

			if (defenseObj.data.hp <= 0) {
				log(defenseObj.data.name + " has been defeated");
			}

			destObj.grid.getModel().valueChanged(defenseObj);
		}

		private function log(str: String) : void {
			txtLog.appendText("\n" + str);

			if (txtLog.getLength() > 1024)
			txtLog.replaceText(0, txtLog.getLength() - 1024, "");

			lstLogScroll.getVerticalScrollBar().setValue(lstLogScroll.getVerticalScrollBar().getMaximum(), true);
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
			setPreferredSize(new IntDimension(965, 405));

			var pnlBody: JPanel = new JPanel(new SoftBoxLayout(SoftBoxLayout.X_AXIS, 0));
			pnlBody.setBorder(null);

			lblStamina = new JLabel("", null, AsWingConstants.RIGHT);

			tabDefensive = new JTabbedPane();
			tabDefensive.setPreferredSize(new IntDimension(280, 350));

			var border1:TitledBorder = new TitledBorder();
			border1.setColor(new ASColor(0x0, 1));
			border1.setTitle("Defender");
			border1.setPosition(1);
			border1.setAlign(AsWingConstants.LEFT);
			border1.setBeveled(true);
			border1.setRound(5);
			tabDefensive.setBorder(border1);

			txtLog = new JTextArea("You have started watching this battle");
			txtLog.setPreferredSize(new IntDimension(375, 350));
			txtLog.setBorder(new EmptyBorder());
			txtLog.setWordWrap(true);
			txtLog.setEditable(false);
			txtLog.setMaxChars(2000);

			var border2:TitledBorder = new TitledBorder();
			border2.setColor(new ASColor(0x0, 1));
			border2.setTitle("Battle Log");
			border2.setPosition(1);
			border2.setBeveled(true);
			border2.setRound(5);
			lstLogScroll = new JScrollPane(txtLog);
			lstLogScroll.setBorder(border2);

			tabOffensive = new JTabbedPane();
			tabOffensive.setPreferredSize(new IntDimension(280, 350));

			var border3:TitledBorder = new TitledBorder();
			border3.setColor(new ASColor(0x0, 1));
			border3.setTitle("Attacker");
			border3.setPosition(1);
			border3.setAlign(AsWingConstants.RIGHT);
			border3.setBeveled(true);
			border3.setRound(5);
			tabOffensive.setBorder(border3);

			//component layoution
			pnlBody.append(tabDefensive);
			pnlBody.append(lstLogScroll);
			pnlBody.append(tabOffensive);

			append(lblStamina);
			append(pnlBody);
		}
	}

}

