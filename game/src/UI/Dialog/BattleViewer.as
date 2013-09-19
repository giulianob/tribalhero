package src.UI.Dialog {

    import flash.display.*;
    import flash.events.Event;
    import flash.utils.Dictionary;

    import mx.utils.*;

    import org.aswing.*;
    import org.aswing.border.*;
    import org.aswing.event.*;
    import org.aswing.geom.*;

    import src.*;
    import src.Objects.Battle.*;
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
		private var pnlProperties:JPanel;

		private var battle: BattleManager;
		private var battleId: int;

		private var combat: Array = [];
		
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
                battle.addEventListener(BattleManager.GROUP_UNIT_ADDED, onGroupUnitAdded);
				battle.addEventListener(BattleManager.OBJECT_ATTACKED, onAttack);
				battle.addEventListener(BattleManager.OBJECT_SKIPPED, onSkipped);
				battle.addEventListener(BattleManager.END, onEnd);
				battle.addEventListener(BattleManager.NEW_ROUND, onNewRound);
				battle.addEventListener(BattleManager.PROPERTIES_CHANGED, onPropertiesChanged);
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
				battle.removeEventListener(BattleManager.PROPERTIES_CHANGED, onPropertiesChanged);

				Global.mapComm.Battle.battleUnsubscribe(battleId);
			}
		}
				
		private function onPropertiesChanged(e:Event):void 
		{
			pnlProperties.removeAll();
			for (var propertyName: String in battle.properties) {
				var lbl: JLabel = new JLabel(StringUtil.substitute("{0}: {1}", StringHelper.localize("BATTLE_PROP_" + propertyName.toUpperCase()), battle.properties[propertyName].toString()), null, AsWingConstants.LEFT);
				GameLookAndFeel.changeClass(lbl, "darkText");
				pnlProperties.append(lbl);
			}
			pnlProperties.setVisible(pnlProperties.getComponentCount() > 0);
		}		

		private function onNewRound(e: BattleRoundEvent = null) : void {
			log(new JSeparator());
			getFrame().setTitle(StringHelper.localize("BATTLE_NEW_ROUND", battle.location.name, (e.round + 1)));
			logStr(StringHelper.localize("BATTLE_ROUND", (e.round + 1)), null, true);
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
			tabPanel.appendTab(tab, StringUtil.substitute("{0} ({1})", combatGroup.owner.name, combatGroup.troopId == 1 ? StringHelper.localize("BATTLE_SIDE_LOCAL") : combatGroup.troopId));
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

			logStr(StringHelper.localize("BATTLE_ENDED"));
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
        
		private function onGroupUnitAdded(e:BattleObjectEvent):void 
		{
			var groupUi: * = tabsByGroup[e.combatGroup.id];
			
			if (!groupUi) {
				Util.log("Received unit added for unknown group");
				return;
			}
			
			var groupList: CombatObjectGridList = groupUi.grid;
			groupList.addCombatObject(e.combatObject);		
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
			var lbl: JLabel = new JLabel(StringHelper.localize("BATTLE_CANT_REACH"), new AssetIcon(groupUi.isAttacker ? new ICON_SINGLE_SWORD : new ICON_SHIELD));
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

			var arrowPnl: JPanel = new JPanel(new FlowLayout(AsWingConstants.CENTER));
			arrowPnl.append(new AssetPane(e.attackerSide == BattleManager.SIDE_DEFENSE ? new ICON_ARROW_RIGHT : new ICON_ARROW_LEFT));
			
			var dmgPnl: JPanel = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 0, AsWingConstants.CENTER));
			dmgPnl.appendAll(
				new JLabel(StringHelper.localize("BATTLE_DAMAGE", e.dmg.toString()), new AssetIcon(e.attackerSide == BattleManager.SIDE_DEFENSE ? new ICON_SHIELD : new ICON_SINGLE_SWORD)),
				arrowPnl
			);

			var pnl: JPanel = new JPanel(new FlowLayout(AsWingConstants.CENTER, 7, 0, false));
			
			var attackerPnl: Component = getCombatObjectPanel(e.attackerCombatGroup, e.attackerCombatObj, e.attackerCount);
			var targetPnl: Component = getCombatObjectPanel(e.targetCombatGroup, e.targetCombatObj, e.targetCount);
			
			// Always keep defensive obj on the left and attacker on the right
			if (e.attackerSide == BattleManager.SIDE_DEFENSE) {
				pnl.appendAll(attackerPnl, dmgPnl, targetPnl);
			}
			else {
				pnl.appendAll(targetPnl, dmgPnl, attackerPnl);
			}

			log(pnl);

			if (defenseObj.data.hp <= 0) {
				pnl = new JPanel(new FlowLayout(AsWingConstants.CENTER, 0, 0, false));
				pnl.append(getCombatObjectPanel(e.targetCombatGroup, e.targetCombatObj));
				var defeatLbl: JLabel = new JLabel(StringHelper.localize("BATTLE_DEFEATED"), new AssetIcon(e.attackerSide == BattleManager.SIDE_ATTACK ? new ICON_SHIELD : new ICON_SINGLE_SWORD));
				defeatLbl.setHorizontalTextPosition(AsWingConstants.LEFT);
				pnl.append(defeatLbl);
				log(pnl);
			}

			targetGroupUi.grid.getModel().valueChanged(defenseObj);
		}
		
		private function getCombatObjectPanel(combatGroup: CombatGroup, combatObj: CombatObject, count: int = 0) : Component {
			var text: String = StringUtil.substitute("{0}({1}):{2}",
                    combatGroup.owner.name,
                    combatGroup.troopId == 1 ? StringHelper.localize("BATTLE_SIDE_LOCAL") : combatGroup.troopId,
                    combatObj.name
            );

            var pnl: JPanel = new JPanel(new SoftBoxLayout());
            if (count > 0) {
                var lblCount: JLabel = new JLabel(count.toString(), null, AsWingConstants.LEFT);
                lblCount.setVerticalAlignment(AsWingConstants.CENTER);
                GameLookAndFeel.changeClass(lblCount, "Label.small");
                pnl.append(lblCount);
            }

            var icon: DisplayObjectContainer = combatObj.getIcon();
            if (icon) {
                Util.resizeSprite(icon, 60, 40);
                pnl.append(new AssetPane(icon));
            }

            pnl.append(new JLabel(text));

            return pnl;
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
			frame.setResizable(true);
			frame.setMinimumSize(new IntDimension(650, 545));
			
			Global.gameContainer.showFrame(frame);
			
			return frame;
		}

		private function createUI(): void {
			setLayout(new BorderLayout());
			setBorder(null);
			setPreferredSize(new IntDimension(650, Math.max(600, Constants.screenH - GameJImagePanelBackground.getFrameHeight())));
			
			var pnlNorth: JPanel = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 5));
			pnlNorth.setConstraints("North");
			{			
				pnlProperties = new JPanel(new FlowLayout(AsWingConstants.LEFT, 15, 5, false));
				pnlProperties.setVisible(false);
				
				tabDefensive = new JTabbedPane();
				tabDefensive.setPreferredHeight(175);
			
				var defenderBorder:SimpleTitledBorder = new SimpleTitledBorder(null, StringHelper.localize("BATTLE_TAB_DEFENDER"), AsWingConstants.TOP, AsWingConstants.LEFT);
				tabDefensive.setBorder(defenderBorder);
				
				pnlNorth.appendAll(pnlProperties, tabDefensive);
			}
			
			pnlLog = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 8, AsWingConstants.CENTER));
			pnlLog.setBorder(new EmptyBorder(null, new Insets(10, 0, 5, 0)));

			lstLogScroll = new JScrollPane(pnlLog);
			lstLogScroll.setBorder(new EmptyBorder(null, new Insets()));

			var tabLog: JTabbedPane = new JTabbedPane();
			tabLog.appendTab(lstLogScroll, StringHelper.localize("BATTLE_TAB_LOG"));

			tabOffensive = new JTabbedPane();
			tabOffensive.setPreferredHeight(175);

			var attackerBorder: SimpleTitledBorder = new SimpleTitledBorder(null, StringHelper.localize("BATTLE_TAB_ATTACKER"), AsWingConstants.TOP, AsWingConstants.LEFT);
			tabOffensive.setBorder(attackerBorder);

			//component layoution			
			tabLog.setConstraints("Center");
			tabOffensive.setConstraints("South");
			
			append(pnlNorth);
			append(tabLog);
			append(tabOffensive);
		}
	}

}

