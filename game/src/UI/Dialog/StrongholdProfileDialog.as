package src.UI.Dialog 
{
	import adobe.utils.CustomActions;
	import com.adobe.images.JPGEncoder;
	import fl.lang.Locale;
	import flash.events.*;
	import flash.utils.*;
	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.colorchooser.*;
	import org.aswing.event.*;
	import org.aswing.ext.*;
	import org.aswing.geom.*;
	import org.aswing.table.*;
	import src.*;
	import src.UI.*;
	import src.UI.Components.*;
	import src.UI.Components.TableCells.*;
	import src.UI.Components.Tribe.*;
	import src.UI.Components.TroopCompositionGridList.TroopCompositionGridList;
	import src.UI.Components.TroopStubGridList.TroopStubGridCell;
	import src.UI.LookAndFeel.*;
	import src.UI.Tooltips.*;
	import src.Map.Username;
	import src.Objects.Troop.*;
	import src.UI.Components.ComplexTroopGridList.*;
	import src.Util.Util;
	
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
			pnlLeftContainer = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS));
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
			pnl.appendAll(	new JButton("Send Reinforcement"),
							new JButton("View Battle"),
							new JButton("Goto Stronghold"));
			
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
			addInfo(form, "Total Troops", new TroopCompositionGridList(s, Formation.Defense, 3, 0));
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
			var pnl: JPanel = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 5));
			for each (var troop: * in profileData.troops) {
				pnl.append(createTroopItem(troop));
			}
			
			var scrollTroops: JScrollPane = new JScrollPane(new JViewport(pnl, true), JScrollPane.SCROLLBAR_ALWAYS, JScrollPane.SCROLLBAR_NEVER);
			(scrollTroops.getViewport() as JViewport).setVerticalAlignment(AsWingConstants.TOP);			
			
			var tabPanel: JTabbedPane = new JTabbedPane();
			tabPanel.appendTab(new JScrollPane(pnl, JScrollPane.SCROLLBAR_ALWAYS, JScrollPane.SCROLLBAR_NEVER), Locale.loadString("STR_TROOPS"));
			tabPanel.setPreferredSize(new IntDimension(400, 300));
			return tabPanel;
		}
		
		private function createReportPanel() : Container {
			var pnl: JPanel = new JPanel();
			pnl.setPreferredSize(new IntDimension(400, 150));
			var tabPanel: JTabbedPane = new JTabbedPane();
			tabPanel.appendTab(pnl, Locale.loadString("STR_REPORTS"));
			return tabPanel;
		}
	}
	
}