package src.UI.Dialog {

	import flash.display.*;
	import flash.events.Event;
	import flash.utils.Dictionary;
	import mx.utils.*;
	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.colorchooser.*;
	import org.aswing.event.*;
	import org.aswing.ext.*;
	import org.aswing.geom.*;
	import src.*;
	import src.Map.*;
	import src.Objects.Battle.*;
	import src.Objects.Factories.*;
	import src.UI.*;
	import src.UI.Components.CombatObjectGridList.*;
	import src.UI.Components.StickyScroll;
	import src.UI.LookAndFeel.*;
	import src.Util.*;
	
	public class BattleViewer extends GameJPanel {

		private var tabDefensive:JTabbedPane;
		private var pnlLog: JPanel;
		private var tabOffensive:JTabbedPane;
		private var lstLogScroll:JScrollPane;

		private var battle: BattleManager;
		private var battleId: int;

		private var combat: Array = new Array();
		
		private var tabsByGroup: Dictionary = new Dictionary();

		public function BattleViewer(battleId: int) {
			createUI();

			title = "Battle Viewer";
			
			new StickyScroll(lstLogScroll);

			this.battleId = battleId;

			battle = Global.mapComm.Battle.battleSubscribe(battleId, this);
			
			if (battle) {
				battle.addEventListener(BattleManager.GROUP_ADDED_ATTACK, onAddedAttack);
				battle.addEventListener(BattleManager.GROUP_REMOVED_ATTACK, onRemoved);
				battle.addEventListener(BattleManager.GROUP_ADDED_DEFENSE, onAddedDefense);
				battle.addEventListener(BattleManager.GROUP_REMOVED_DEFENSE, onRemoved);
				battle.addEventListener(BattleManager.GROUP_UNIT_REMOVED, onGroupUnitRemoved);
				battle.addEventListener(BattleManager.OBJECT_ATTACKED, onAttack);
				battle.addEventListener(BattleManager.OBJECT_SKIPPED, onSkipped);
				battle.addEventListener(BattleManager.END, onEnd);
				battle.addEventListener(BattleManager.NEW_ROUND, onNewRound);
			}
		}
		
		public function onClosed(e: *):void
		{
			if (battle)
			{
				battle.removeEventListener(BattleManager.GROUP_ADDED_ATTACK, onAddedAttack);
				battle.removeEventListener(BattleManager.GROUP_REMOVED_ATTACK, onRemoved);
				battle.removeEventListener(BattleManager.GROUP_UNIT_REMOVED, onGroupUnitRemoved);
				battle.removeEventListener(BattleManager.GROUP_ADDED_DEFENSE, onAddedDefense);
				battle.removeEventListener(BattleManager.GROUP_REMOVED_DEFENSE, onRemoved);
				battle.removeEventListener(BattleManager.OBJECT_ATTACKED, onAttack);
				battle.removeEventListener(BattleManager.OBJECT_SKIPPED, onSkipped);
				battle.removeEventListener(BattleManager.END, onEnd);
				battle.removeEventListener(BattleManager.NEW_ROUND, onNewRound);

				Global.mapComm.Battle.battleUnsubscribe(battleId);
			}
		}

		private function onNewRound(e: BattleRoundEvent = null) : void {
			log(new JSeparator());
			getFrame().setTitle(StringUtil.substitute("Battle - {0} - Round {1}", battle.location.name, (e.round + 1)));
			logStr("Round " + (e.round + 1), null, true);
		}
		
		private function addGroup(combatGroup: CombatGroup, defense: Boolean) : void {
			
			// If group already exists then return
			if (tabsByGroup[combatGroup.id]) {
				return;
			}
			
			var grid: CombatObjectGridList = CombatObjectGridList.getGridList(combatGroup.toArray());
			var tab: JScrollPane = new JScrollPane(grid);

			tabsByGroup[combatGroup.id] = { tab: tab, grid: grid, isAttacker: !defense };
					
			var tabPanel: JTabbedPane = defense ? tabDefensive : tabOffensive;
			tabPanel.appendTab(tab, StringUtil.substitute("{0} ({1})", combatGroup.owner.name, combatGroup.troopId == 1 ? "Local" : combatGroup.troopId));
		}

		private function removeGroup(groupId: int) : void {
			var groupUi:* = tabsByGroup[groupId];
			
			if (!groupUi) { 
				return;
			}
			
			if (groupUi.isAttacker) {
				tabOffensive.remove(groupUi.tab);
			}
			else {
				tabDefensive.remove(groupUi.tab);
			}
		}

		private function findObject(gridList: CombatObjectGridList, combatObjectId: int) : * {
			var listData: VectorListModel = gridList.getModel() as VectorListModel;
			for (var i: int = 0; i < listData.size(); i++) {
				var combatObj: * = listData.getElementAt(i);
				if (combatObj.data.combatObjectId == combatObjectId) {
					return combatObj;
				}
			}

			return null;
		}

		public function onEnd(e: Event):void
		{
			tabOffensive.removeAll();
			tabDefensive.removeAll();
			tabsByGroup = new Dictionary();

			logStr("Battle has ended");
		}

		public function onAddedAttack(e: BattleGroupEvent):void
		{			
			addGroup(e.combatGroup, false);
		}

		public function onRemoved(e: BattleGroupEvent):void
		{		
			removeGroup(e.combatGroup.id);
		}

		public function onAddedDefense(e: BattleGroupEvent):void
		{
			addGroup(e.combatGroup, true);
		}

		private function onGroupUnitRemoved(e:BattleObjectEvent):void 
		{
			var groupUi: * = tabsByGroup[e.combatGroup.id];
			
			if (!groupUi) {
				Util.log("Received unit removed for unknown group");
				return;
			}
			
			var combatObject: * = findObject(groupUi.grid, e.combatObject.combatObjectId);
			var groupListModel: VectorListModel = groupUi.grid.getModel();
			groupListModel.remove(combatObject);		
			if (groupListModel.size() == 0) {
				removeGroup(e.combatGroup.id);
			}
		}		
		
		public function onSkipped(e: BattleObjectEvent) : void {
			var groupUi: * = tabsByGroup[e.combatGroup.id];
			
			if (!groupUi) {
				Util.log("Received skip for unknown group");
				return;
			}

			var attackObj:* = findObject(groupUi.grid, e.combatObject.combatObjectId);

			if (attackObj == null) {
				Util.log("Could not find attacker combat object");
				return;
			}

			var pnl: JPanel = new JPanel(new FlowLayout(AsWingConstants.CENTER, 0, 0, false));
			pnl.append(getCombatObjectPanel(e.combatGroup, e.combatObject));
			var lbl: JLabel = new JLabel("couldn't reach anyone", new AssetIcon(groupUi.isAttacker ? new ICON_SINGLE_SWORD : new ICON_SHIELD));
			lbl.setHorizontalTextPosition(AsWingConstants.LEFT);
			pnl.append(lbl);

			log(pnl);
		}
		
		public function onAttack(e: BattleAttackEvent):void
		{
			var targetGroupUi: * = tabsByGroup[e.targetCombatGroup.id];
			var attackerGroupUi: * = tabsByGroup[e.attackerCombatGroup.id];

			if (!targetGroupUi || !attackerGroupUi) {
				Util.log("Received attack for unknown object");
				return;
			}

			//find attacker
			var defenseObj: * = findObject(targetGroupUi.grid, e.targetCombatObj.combatObjectId);
			var attackObj:* = findObject(attackerGroupUi.grid, e.attackerCombatObj.combatObjectId);

			if (!attackObj || !defenseObj) {
				Util.log("Could not find combat objects");
				return;
			}

			var dmgPnl: JPanel = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 0, AsWingConstants.CENTER));
			dmgPnl.append(new JLabel(e.dmg.toString() + " dmg", new AssetIcon(e.attackerSide == BattleManager.SIDE_DEFENSE ? new ICON_SHIELD : new ICON_SINGLE_SWORD)));
			var arrowPnl: JPanel = new JPanel(new FlowLayout(AsWingConstants.CENTER));
			arrowPnl.append(new AssetPane(e.attackerSide == BattleManager.SIDE_DEFENSE ? new ICON_ARROW_RIGHT : new ICON_ARROW_LEFT));
			dmgPnl.append(arrowPnl);

			var pnl: JPanel = new JPanel(new FlowLayout(AsWingConstants.CENTER, 7, 0, false));
			
			pnl.append(getCombatObjectPanel(e.attackerCombatGroup, e.attackerCombatObj));
			pnl.append(dmgPnl);
			pnl.append(getCombatObjectPanel(e.targetCombatGroup, e.targetCombatObj));			

			log(pnl);

			if (defenseObj.data.hp <= 0) {
				pnl = new JPanel(new FlowLayout(AsWingConstants.CENTER, 0, 0, false));
				pnl.append(getCombatObjectPanel(e.targetCombatGroup, e.targetCombatObj));
				var defeatLbl: JLabel = new JLabel("has been defeated", new AssetIcon(e.attackerSide == BattleManager.SIDE_ATTACK ? new ICON_SHIELD : new ICON_SINGLE_SWORD));
				defeatLbl.setHorizontalTextPosition(AsWingConstants.LEFT);
				pnl.append(defeatLbl);
				log(pnl);
			}

			targetGroupUi.grid.getModel().valueChanged(defenseObj);
		}
		
		private function getCombatObjectPanel(combatGroup: CombatGroup, combatObj: CombatObject) : Component {
			var text: String = StringUtil.substitute("{0}({1}):{2}", combatGroup.owner.name, combatGroup.troopId == 1 ? "Local" : combatGroup.troopId, combatObj.name);
			var icon: DisplayObjectContainer = combatObj.getIcon();
			return new JLabel(text, (icon != null ? new AssetIcon(icon) : null));
		}

		private function logStr(string: String, icon: Icon = null, header: Boolean = false) : void {
			var txt: JLabel = new JLabel(string, icon);
			if (header) GameLookAndFeel.changeClass(txt, "darkHeader");
			log(txt);
		}

		private function log(item: Component) : void {
			pnlLog.append(item);

			if (pnlLog.getComponentCount() > 100) {
				pnlLog.removeAt(0);
			}

			pnlLog.pack();
		}

		public function show(owner:* = null, modal:Boolean = true, onClose:Function = null):JFrame
		{
			super.showSelf(owner, modal, onClose);

			frame.addEventListener(PopupEvent.POPUP_CLOSED, onClosed);

			if (modal) {
				Global.gameContainer.showFrame(frame);
			}
			else {
				frame.show();
			}
			
			frame.setResizable(true);
			frame.setMinimumSize(new IntDimension(650, 545));
			
			return frame;
		}

		private function createUI(): void {
			setLayout(new BorderLayout());
			setBorder(null);
			setPreferredSize(new IntDimension(650, Math.max(600, Constants.screenH - GameJImagePanelBackground.getFrameHeight())));

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

			tabOffensive = new JTabbedPane();
			tabOffensive.setPreferredHeight(175);

			var border3: SimpleTitledBorder = new SimpleTitledBorder(null, "Attacker", AsWingConstants.TOP, AsWingConstants.LEFT);
			tabOffensive.setBorder(border3);

			//component layoution
			tabDefensive.setConstraints("North");
			tabLog.setConstraints("Center");
			tabOffensive.setConstraints("South");
			
			append(tabDefensive);
			append(tabLog);
			append(tabOffensive);
		}
	}

}

