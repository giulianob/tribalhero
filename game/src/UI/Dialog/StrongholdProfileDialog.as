﻿package src.UI.Dialog 
{
	import fl.lang.*;
	import flash.events.*;
	import flash.geom.*;
	import flash.utils.*;
	import mx.utils.StringUtil;
	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.colorchooser.*;
	import org.aswing.event.*;
	import org.aswing.ext.*;
	import org.aswing.geom.*;
	import org.aswing.table.*;
	import src.*;
	import src.Map.*;
	import src.Objects.*;
	import src.Objects.Battle.*;
	import src.Objects.Process.*;
	import src.Objects.Troop.*;
	import src.UI.*;
	import src.UI.Components.*;
	import src.UI.Components.BattleReport.*;
	import src.UI.Components.ComplexTroopGridList.*;
	import src.UI.Components.TableCells.*;
	import src.UI.Components.Tribe.*;
	import src.UI.Components.TroopCompositionGridList.*;
	import src.UI.LookAndFeel.*;
	import src.UI.Tooltips.*;
	import src.Util.*;
	
	public class StrongholdProfileDialog extends GameJPanel
	{
		private var profileData: * ;
		
		private var pnlInfoContainer: Container;
		private var pnlButtonContainer: Container;
		private var pnlLeftContainer: Container;

		private var pnlTroopPanel: Container;
		private var pnlReportPanel: Container;
		private var pnlRightContainer: Container;
		
		private var lblStrongholdName: JLabel;
		
		private var reports: LocalReportList;
		private var nameLabel: JLabel;
		
		private var btnGoTo: JLabelButton;
		private var btnSendReinforcement: JLabelButton;
		private var btnViewBattle: JLabelButton;
		private var pnlHeaderButtons:JPanel;
				
		public function StrongholdProfileDialog(profileData: *) 
		{
			this.profileData = profileData;
			createUI();
			
			var self: StrongholdProfileDialog = this;
			btnGoTo.addActionListener(function(e: Event):void {
				Global.gameContainer.closeAllFrames(true);
				var pt:Point = MapUtil.getScreenCoord(self.profileData.strongholdX, self.profileData.strongholdY);
				Global.map.camera.ScrollToCenter(pt.x, pt.y);
			});
			
			btnSendReinforcement.addActionListener(function(e:Event): void {
				var process : ReinforcementSendProcess = new ReinforcementSendProcess(new Location(Location.STRONGHOLD, self.profileData.strongholdId));
				process.execute();
			});			
			
			btnViewBattle.addActionListener(function(e: Event): void {
				var battleViewer: BattleViewer = new BattleViewer(self.profileData.strongholdBattleId);
				battleViewer.show(null, false);
			});			
		}
		
		public function show(owner:* = null, modal:Boolean = true, onClose: Function = null):JFrame 
		{
			super.showSelf(owner, modal, onClose);
			Global.gameContainer.closeAllFramesByType(StrongholdProfileDialog);
			Global.gameContainer.showFrame(frame);
			return frame;
		}
		
		private function createUI():void {
			setPreferredHeight(500);
			
			title = "Stronghold Profile - " + profileData.strongholdName;
			setLayout(new SoftBoxLayout(SoftBoxLayout.X_AXIS));
			
			nameLabel = new JLabel(profileData.strongholdName, null, AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(nameLabel, "darkSectionHeader");
			
			// stronghold properties panel
			pnlInfoContainer = createInfoPanel();
			
			// button panel
			pnlButtonContainer = createButtonPanel();
			
			// Troop panel
			pnlTroopPanel = createTroopPanel();
			
			// Report panel
			pnlReportPanel = createReportPanel();
			
			// Left panel
			pnlLeftContainer = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 0));
			pnlLeftContainer.setPreferredWidth(225);			
			pnlLeftContainer.appendAll(nameLabel, pnlButtonContainer, pnlInfoContainer);
			
			// Right panel			
			var tabPanel: JTabbedPane = new JTabbedPane();
			tabPanel.setPreferredWidth(500);
			tabPanel.appendTab(pnlTroopPanel, StringHelper.localize("STR_TROOPS"));
			tabPanel.appendTab(pnlReportPanel, StringHelper.localize("STR_REPORTS"));			

			// Append main panels
			appendAll(pnlLeftContainer, tabPanel);
		}
		
		private function addInfo(form: Form, title: String, textOrComponent: *, icon: Icon = null) : void {
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

			form.addRow(rowTitle, rowValue);
		}
		
		private function createButtonPanel() : Container {
			if (!pnlHeaderButtons) {
				pnlHeaderButtons = new JPanel(new FlowLayout(AsWingConstants.LEFT, 10, 0, false));
				pnlHeaderButtons.setBorder(new EmptyBorder(null, new Insets(0, 0, 10)));
				
				btnSendReinforcement = new JLabelButton("Send reinforcement");

				btnGoTo = new JLabelButton("Go there");
				
				btnViewBattle = new JLabelButton("View battle");
				
				pnlHeaderButtons.appendAll(btnSendReinforcement, btnGoTo, btnViewBattle);
			}
			
			btnViewBattle.setVisible(profileData.strongholdObjectState == SimpleGameObject.STATE_BATTLE);
			
			return pnlHeaderButtons;
		}
   
		private function createInfoPanel() : Container {
			var form: Form = new Form();
			addInfo(form, Locale.loadString("STR_LEVEL"), profileData.strongholdLevel.toString());
			addInfo(form, Locale.loadString("STR_GATE"), profileData.strongholdGate.toString());
			
			var timediff :int = Global.map.getServerTime() - profileData.strongholdDateOccupied;
			addInfo(form, Locale.loadString("STR_OCCUPIED"), Util.niceDays(timediff));
			addInfo(form, Locale.loadString("STR_VP_RATE"), StringUtil.substitute(Locale.loadString("STR_PER_DAY_RATE"), Util.roundNumber(profileData.strongholdVictoryPointRate).toString()));
		
			var s:TroopStub = new TroopStub();
			var f:Formation = new Formation(Formation.Defense);
			for each (var troop: * in profileData.troops) {
				for each(var formation: Formation in troop.stub.each())
				{
					if (formation.type!=Formation.Defense || formation.size() == 0) continue;
					for (var z: int = 0; z < formation.size(); z++)
					{
						var u:Unit = formation.getByIndex(z);
						var newUnit:Unit = new Unit(u.type, u.count);
						f.add(newUnit);
					}
				}
			}
			s.add(f);
			
			if(f.size()>0)
				addInfo(form, Locale.loadString("STR_TOTAL_TROOPS"), new TroopCompositionGridList(s, Formation.Defense, 3, 0));
			else 
				addInfo(form, Locale.loadString("STR_TOTAL_TROOPS"), Locale.loadString("STR_NONE_DEFENDING"));
				
			return form;
		}
		
		private function createTroopItem(troop: *) : JPanel {
			var pnl: JPanel = new JPanel(new BorderLayout(5));
			
			var label: PlayerCityLabel = new PlayerCityLabel(troop.playerId, troop.cityId, troop.playerName, troop.cityName);
			label.setPreferredWidth(175);
			label.setConstraints("West");
			pnl.append(label);
			
			var gridList: TroopCompositionGridList = new TroopCompositionGridList(troop.stub, Formation.Defense, 5, 0);
			gridList.setConstraints("Center");
			
			pnl.append(AsWingUtils.createPaneToHold(gridList, new SoftBoxLayout(), "Center"));
			
			if (troop.playerId == Constants.playerId) {
				var retreatButton: JLabelButton = new JLabelButton(Locale.loadString("STR_RETREAT"), null, AsWingConstants.LEFT);
				retreatButton.setVerticalAlignment(AsWingConstants.TOP);
				
				var cityId: uint = troop.cityId;
				var troopId: int = troop.stub.id;
				retreatButton.addActionListener(function(e: Event):void {
					InfoDialog.showMessageDialog("Confirm", "Are you sure? Retreating will bring your troop back to your city.", function(result: int): void {				
						if (result == JOptionPane.YES) {
							Global.mapComm.Troop.retreat(cityId, troopId);
						}
					}, null, true, true, JOptionPane.YES | JOptionPane.NO);		
				},0, true);
				
				var pnlButtons: JPanel = new JPanel(new FlowLayout(AsWingConstants.RIGHT, 5, 0, false));
				retreatButton.setVerticalAlignment(AsWingConstants.TOP);
				pnlButtons.setConstraints("East");								
				pnlButtons.appendAll(retreatButton);
				pnl.append(pnlButtons);
			}
			
			return pnl;
		}
		
		private function createTroopPanel() : Container {
			var pnl: JPanel = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 5, AsWingConstants.TOP));
			
			for each (var troop: * in profileData.troops) {
				pnl.append(createTroopItem(troop));
			}
			
			if (profileData.troops.length == 0) {
				pnl.append(new JLabel(Locale.loadString("STRONGHOLD_NO_TROOPS"), null, AsWingConstants.LEFT));
			}
			
			var tabScrollPanel: JScrollPane = new JScrollPane(new JViewport(pnl, true), JScrollPane.SCROLLBAR_ALWAYS, JScrollPane.SCROLLBAR_NEVER);			
			(tabScrollPanel.getViewport() as JViewport).setVerticalAlignment(AsWingConstants.TOP);

			return pnl;
		}
		
		private function createReportPanel() : Container {
			reports = new LocalReportList(
				BattleReportViewer.REPORT_TRIBE_LOCAL, 
				[BattleReportListTable.COLUMN_DATE, BattleReportListTable.COLUMN_LOCATION, BattleReportListTable.COLUMN_ATTACK_TRIBES], 
				new BattleLocation(BattleLocation.STRONGHOLD, profileData.strongholdId));
							
			return reports;
		}
	}
	
}