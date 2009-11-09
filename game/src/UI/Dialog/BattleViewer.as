package src.UI.Dialog {

import org.aswing.*;
import org.aswing.border.*;
import org.aswing.event.FrameEvent;
import org.aswing.event.PopupEvent;
import org.aswing.geom.*;
import org.aswing.colorchooser.*;
import org.aswing.ext.*;
import src.Global;
import src.Map.Username;
import src.UI.Components.CombatObjectGridList.CombatObjectGridList;
import src.UI.GameJPanel;
import src.Objects.Battle.*;
	
public class BattleViewer extends GameJPanel {
	
	private var tabDefensive:JTabbedPane;
	private var lstLog:JList;
	private var tabOffensive:JTabbedPane;
	private var lstLogScroll:JScrollPane;
	
	private var battle: BattleManager;
	private var battleCityId: int;
	
	private var combat: Array = new Array();
	
	public function BattleViewer(battleCityId: int) {
		createUI();
		
		title = "Battle Viewer";
		
		this.battleCityId = battleCityId;
		
		battle = Global.map.mapComm.Battle.battleSubscribe(battleCityId);
		battle.addEventListener(BattleManager.OBJECT_ADDED_ATTACK, onAddedAttack);
		battle.addEventListener(BattleManager.OBJECT_REMOVED_ATTACK, onRemoved);
		battle.addEventListener(BattleManager.OBJECT_ADDED_DEFENSE, onAddedDefense);
		battle.addEventListener(BattleManager.OBJECT_REMOVED_DEFENSE, onRemoved);			
		battle.addEventListener(BattleManager.OBJECT_ATTACKED, onAttack);
		battle.addEventListener(BattleManager.END, onEnd);
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
			battle.removeEventListener(BattleManager.END, onEnd);
			
			Global.map.mapComm.Battle.battleUnsubscribe(battleCityId);
		}
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
	
	public function onEnd(e: BattleEvent):void 
	{
		tabOffensive.removeAll();
		tabDefensive.removeAll();
		
		log("Battle has ended");
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
	
	public function onAttack(e: BattleEvent):void
	{
		var destObj: Object = findTab(e.destCombatObj.cityId, e.destCombatObj.troopStubId);		
		var srcObj: Object = findTab(e.combatObj.cityId, e.combatObj.troopStubId);
		
		if (srcObj == null || destObj == null) {
			trace("Received attack for unknown object");
			return;
		}
		
		//find attacker
		var attackObj: * = null;
		var listData: VectorListModel = (srcObj.grid as CombatObjectGridList).getModel() as VectorListModel;				
		for (var i: int = 0; i < listData.size(); i++) {
			var combatObj: Object = listData.getElementAt(i); 
			if (combatObj.data.combatObjectId == e.combatObj.combatObjectId) {
				attackObj = combatObj;
				break;
			}
		}
		
		if (attackObj == null) {
			trace("Could not find attacker combat object");
			return;
		}
		
		//find defender
		var defenseObj: * = null;
		listData = (destObj.grid as CombatObjectGridList).getModel() as VectorListModel;				
		for (i = 0; i < listData.size(); i++) {
			combatObj = listData.getElementAt(i); 
			if (combatObj.data.combatObjectId == e.destCombatObj.combatObjectId) {
				defenseObj = combatObj;
				break;
			}
		}
		
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
		
		listData.valueChanged(defenseObj);
		
		lstLogScroll.getVerticalScrollBar().setValue(lstLogScroll.getVerticalScrollBar().getMaximum());
	}
	
	private function log(str: String) : void {
		(lstLog.getModel() as VectorListModel).append(str);
		
		var size: int = (lstLog.getModel() as VectorListModel).size();
		if (size > 200)
			(lstLog.getModel() as VectorListModel).removeRange(0, size - 200);				
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
		layout0.setAxis(AsWingConstants.HORIZONTAL);
		setLayout(layout0);
		
		tabDefensive = new JTabbedPane();
		tabDefensive.setLocation(new IntPoint(125, 15));
		tabDefensive.setSize(new IntDimension(300, 350));
		tabDefensive.setPreferredSize(new IntDimension(280, 350));
		var border1:TitledBorder = new TitledBorder();
		border1.setColor(new ASColor(0x0, 1));
		border1.setTitle("Defensive");
		border1.setPosition(1);
		border1.setAlign(AsWingConstants.LEFT);
		border1.setBeveled(true);
		border1.setRound(5);
		tabDefensive.setBorder(border1);
		
		lstLog = new JList();
		lstLog.setLocation(new IntPoint(250, 0));
		lstLog.setSize(new IntDimension(450, 250));
		var border2:TitledBorder = new TitledBorder();
		border2.setColor(new ASColor(0x0, 1));
		border2.setTitle("Battle Log");
		border2.setPosition(1);
		border2.setBeveled(true);
		border2.setRound(5);		
		lstLog.setModel(new VectorListModel());
		lstLog.setPreferredSize(new IntDimension(375, 0));			
		lstLogScroll = new JScrollPane(lstLog);
		lstLogScroll.setBorder(border2);
		
		tabOffensive = new JTabbedPane();
		tabOffensive.setLocation(new IntPoint(500, 0));
		tabOffensive.setSize(new IntDimension(280, 350));
		tabOffensive.setPreferredSize(new IntDimension(280, 350));
		var border3:TitledBorder = new TitledBorder();
		border3.setColor(new ASColor(0x0, 1));
		border3.setTitle("Offensive");
		border3.setPosition(1);
		border3.setAlign(AsWingConstants.RIGHT);
		border3.setBeveled(true);
		border3.setRound(5);
		tabOffensive.setBorder(border3);
		
		//component layoution
		append(tabDefensive);
		append(lstLogScroll);
		append(tabOffensive);		
	}
}
	
}