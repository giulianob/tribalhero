package src.UI.Dialog 
{
	import fl.lang.*;
	import flash.events.*;
	import flash.geom.*;
	import flash.utils.*;
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
				
		public function StrongholdProfileDialog(profileData: *) 
		{
			this.profileData = profileData;
			createUI();
		}

		public function show(owner:* = null, modal:Boolean = true, onClose: Function = null):JFrame 
		{
			super.showSelf(owner, modal, onClose);
			Global.gameContainer.closeAllFramesByType(StrongholdProfileDialog);
			Global.gameContainer.showFrame(frame);
			return frame;
		}
		
		private function createUI():void {
			//setPreferredSize(new IntDimension(Math.min(375, Constants.screenW - GameJImagePanelBackground.getFrameWidth()) , Math.min(600, Constants.screenH - GameJImagePanelBackground.getFrameHeight())));
			
			title = "Stronghold Profile - " + profileData.strongholdName;
			setLayout(new SoftBoxLayout(SoftBoxLayout.X_AXIS));
			
			// stronghold properties panel
			pnlInfoContainer = createInfoPanel();
			
			// button panel
			pnlButtonContainer = createButtonPanel();
			
			// Troop panel
			pnlTroopPanel = createTroopPanel();
			
			// Report panel
			pnlReportPanel = createReportPanel();
			
			// Left panel
			pnlLeftContainer = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS,20));
			pnlLeftContainer.appendAll(pnlInfoContainer, pnlButtonContainer);
			
			// Right panel
			pnlRightContainer = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS));
			pnlRightContainer.appendAll(pnlTroopPanel, pnlReportPanel);

			// Append main panels
			appendAll(pnlLeftContainer, pnlRightContainer);
		}
		
		private function addInfo(form: Form, title: String, textOrComponent: *, icon: Icon = null) : * {
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

			return rowValue;
		}
		
		private function createButtonPanel() : Container {
			var pnl: JPanel = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 10, AsWingConstants.EAST));
			
			var btnGotoStronghold: JButton = new JButton("Go to stronghold");
			btnGotoStronghold.addActionListener(function(e: Event):void {
				Global.gameContainer.closeAllFrames(true);
				var pt:Point = MapUtil.getScreenCoord(profileData.strongholdX, profileData.strongholdY);
				Global.map.camera.ScrollToCenter(pt.x, pt.y);
			});
			pnl.append(btnGotoStronghold);
			
			var btnSendReinforcement: JButton = new JButton("Send reinforcement");
			btnSendReinforcement.addActionListener(function(e:Event): void {
				var process : ReinforcementSendProcess = new ReinforcementSendProcess(new Location(Location.STRONGHOLD, profileData.strongholdId));
				process.execute();
			});
			pnl.append(btnSendReinforcement);
			
			if(profileData.strongholdObjectState==SimpleGameObject.STATE_BATTLE) {
				var btnViewBattle: JButton = new JButton("View battle");
				btnViewBattle.addActionListener(function(e: Event): void {
					var battleViewer: BattleViewer = new BattleViewer(profileData.strongholdBattleId);
					battleViewer.show(null, false);
				});
				pnl.append(btnViewBattle);
			}
			
			return pnl;
		}

		private function createInfoPanel() : Container {
			
			
			var form: Form = new Form();
			addInfo(form, "Name", profileData.strongholdName);
			addInfo(form, "Level", profileData.strongholdLevel.toString());
			addInfo(form, "Gate", profileData.strongholdGate.toString());
			
			var timediff :int = Global.map.getServerTime() - profileData.strongholdDateOccupied;
			addInfo(form, "Occupied", Util.niceDays(timediff));
			addInfo(form, "Victory Point Rate", Util.roundNumber(profileData.strongholdVictoryPointRate).toString()+ " per day");
		
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
				addInfo(form, "Total Troops", new TroopCompositionGridList(s, Formation.Defense, 3, 0));
			else 
				addInfo(form, "Total Troops", "No troop is currently defending!");
			return form;
		}
		
		private function createTroopItem(troop: *) : JPanel {
			var pnl: JPanel = new JPanel(new BorderLayout(5));
			
			var label: PlayerCityLabel = new PlayerCityLabel(troop.playerId, troop.cityId, troop.playerName, troop.cityName);
			label.setConstraints("West");
			pnl.append(label);
			
			//var tilelists: Array = ComplexTroopGridList.getGridList(troop.stub);
			var gridList: TroopCompositionGridList = new TroopCompositionGridList(troop.stub, Formation.Defense, 5, 0);
			gridList.setVerticalAlignment(AsWingConstants.TOP);
			pnl.append(gridList);
			
			if (troop.playerId == Constants.playerId) {
				var btn: JLabelButton = new JLabelButton("Retreat", null, AsWingConstants.RIGHT);
				var cityId: uint = troop.cityId;
				var troopId: int = troop.stub.id;
				btn.addActionListener(function(e: Event):void {
					InfoDialog.showMessageDialog("Confirm", "Are you sure? Retreating will bring your troop back to your city..", function(result: int): void {				
						if (result == JOptionPane.YES) {
							Global.mapComm.Troop.retreat(cityId, troopId);
						}
					}, null, true, true, JOptionPane.YES | JOptionPane.NO);		
				},0,true);
				btn.setConstraints("East");
				btn.setVerticalAlignment(AsWingConstants.TOP);
				pnl.append(btn);
			}
			return pnl;
		}
		
		private function createTroopPanel() : Container {
			var pnl: JPanel = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 5,AsWingConstants.TOP));
			for each (var troop: * in profileData.troops) {
				pnl.append(createTroopItem(troop));
			}
			
			var tabScrollPanel: JScrollPane = new JScrollPane(pnl, JScrollPane.SCROLLBAR_ALWAYS, JScrollPane.SCROLLBAR_NEVER);
			(tabScrollPanel.getViewport() as JViewport).setVerticalAlignment(AsWingConstants.TOP);

			var tabPanel: JTabbedPane = new JTabbedPane();
			tabPanel.appendTab(tabScrollPanel, Locale.loadString("STR_TROOPS"));
			tabPanel.setPreferredSize(new IntDimension(400, 300));
			return tabPanel;
		}
		
		private function createReportPanel() : Container {
			reports = new LocalReportList(BattleReportViewer.REPORT_TRIBE_LOCAL, new BattleLocation(BattleLocation.STRONGHOLD, profileData.strongholdId));
			reports.setConstraints("Center");
			
			var pnl: JPanel = new JPanel(new BorderLayout());
			pnl.append(reports);
			
			pnl.setPreferredSize(new IntDimension(400, 150));
			var tabPanel: JTabbedPane = new JTabbedPane();
			tabPanel.appendTab(pnl, Locale.loadString("STR_REPORTS"));
			
			return tabPanel;
		}
	}
	
}