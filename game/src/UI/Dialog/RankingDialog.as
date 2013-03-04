package src.UI.Dialog{

	import flash.events.*;
	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.colorchooser.*;
	import org.aswing.event.*;
	import org.aswing.ext.*;
	import org.aswing.geom.*;
	import org.aswing.table.*;
	import src.*;
	import src.Comm.*;
	import src.UI.*;
	import src.UI.Components.*;
	import src.UI.Components.Stronghold.DaysOccupiedRankTranslator;
	import src.UI.Components.TableCells.*;

	public class RankingDialog extends GameJPanel {

		private var rankings: Array = [
		{name: "Attack Points", baseOn: "city"},
		{name: "Defense Points", baseOn: "city"},
		{name: "Resources Stolen", baseOn: "city"},
		{name: "Influence Points", baseOn: "city"},
		{name: "Attack Points", baseOn: "player"},
		{name: "Defense Points", baseOn: "player"},
		{name: "Resources Stolen", baseOn: "player"},
		{name: "Influence Points", baseOn: "player"},
		{name: "Level", baseOn: "tribe"},
		{name: "Attack Points", baseOn: "tribe"},
		{name: "Defense Points", baseOn: "tribe"},
		{name: "Victory Points", baseOn: "tribe" },
		{name: "Victory Point Rate", baseOn: "tribe" },
		{name: "Level", baseOn: "stronghold"},
		{name: "Days Occupied", baseOn: "stronghold" },
		{name: "Victory Point Rate", baseOn: "stronghold" },
		];

		private var loader: GameURLLoader;		
		private var type: int = 0;
		
		private var pagingBar: PagingBar;

		private var rankingList: VectorListModel;
		private var rankingModel: PropertyTableModel;
		private var rankingTable: JTable;

		private var tabs: JTabbedPane;
		private var cityRanking: JPanel;
		private var cityAttackRanking: JToggleButton;
		private var cityDefenseRanking: JToggleButton;
		private var cityLootRanking: JToggleButton;
		private var cityInfluenceRanking: JToggleButton;

		private var playerRanking: JPanel;
		private var playerAttackRanking: JToggleButton;
		private var playerDefenseRanking: JToggleButton;
		private var playerLootRanking: JToggleButton;
		private var playerInfluenceRanking: JToggleButton;

		private var tribeRanking: JPanel;
		private var tribeLevelRanking: JToggleButton;
		private var tribeAttackRanking: JToggleButton;
		private var tribeDefenseRanking: JToggleButton;
		private var tribeVictoryRanking: JToggleButton;
		private var tribeVictoryRateRanking: JToggleButton;
		
		private var strongholdRanking: JPanel;
		private var strongholdLevelRanking: JToggleButton;
		private var strongholdOccupiedRanking: JToggleButton;
		private var strongholdVictoryRateRanking: JToggleButton;

		private var txtSearch: JTextField;
		private var btnSearch: JButton;
		
		private var rankingScroll: JScrollPane;

		public function RankingDialog() {
			loader = new GameURLLoader();
			loader.addEventListener(Event.COMPLETE, onLoadRanking);

			createUI();

			// Disables editing the table
			rankingTable.addEventListener(TableCellEditEvent.EDITING_STARTED, function(e: TableCellEditEvent) : void {
				rankingTable.getCellEditor().cancelCellEditing();
			});
			
			// Any special row selection stuff goes here
			rankingTable.addEventListener(SelectionEvent.COLUMN_SELECTION_CHANGED, onSelectionChange);
			rankingTable.addEventListener(SelectionEvent.ROW_SELECTION_CHANGED, onSelectionChange);

			// Tooltips
			new SimpleTooltip(cityAttackRanking, "Sort by attack points");
			new SimpleTooltip(cityDefenseRanking, "Sort by defense points");
			new SimpleTooltip(cityLootRanking, "Sort by total loot stolen");
			new SimpleTooltip(cityInfluenceRanking, "Sort by influence points");
			new SimpleTooltip(playerAttackRanking, "Sort by attack points");
			new SimpleTooltip(playerDefenseRanking, "Sort by defense points");
			new SimpleTooltip(playerLootRanking, "Sort by total loot stolen");
			new SimpleTooltip(playerInfluenceRanking, "Sort by influence points");
			new SimpleTooltip(tribeLevelRanking, "Sort by level");
			new SimpleTooltip(tribeAttackRanking, "Sort by attack points");
			new SimpleTooltip(tribeDefenseRanking, "Sort by defense points");
			new SimpleTooltip(tribeVictoryRanking, "Sort by victory points");
			new SimpleTooltip(tribeVictoryRateRanking, "Sort by victory points generation rate");
			new SimpleTooltip(strongholdLevelRanking, "Sort by level");
			new SimpleTooltip(strongholdOccupiedRanking, "Sort by days occupied");		
			new SimpleTooltip(strongholdVictoryRateRanking, "Sort by victory points generation rate");
			
			// Handle different buttons being pressed
			cityAttackRanking.addActionListener(onChangeRanking);
			cityDefenseRanking.addActionListener(onChangeRanking);
			cityLootRanking.addActionListener(onChangeRanking);
			cityInfluenceRanking.addActionListener(onChangeRanking);

			playerAttackRanking.addActionListener(onChangeRanking);
			playerDefenseRanking.addActionListener(onChangeRanking);
			playerLootRanking.addActionListener(onChangeRanking);
			playerInfluenceRanking.addActionListener(onChangeRanking);

			tribeLevelRanking.addActionListener(onChangeRanking);
			tribeAttackRanking.addActionListener(onChangeRanking);
			tribeDefenseRanking.addActionListener(onChangeRanking);
			tribeVictoryRanking.addActionListener(onChangeRanking);
			tribeVictoryRateRanking.addActionListener(onChangeRanking);
			
			strongholdLevelRanking.addActionListener(onChangeRanking);
			strongholdOccupiedRanking.addActionListener(onChangeRanking);
			strongholdVictoryRateRanking.addActionListener(onChangeRanking);

			btnSearch.addActionListener(onSearch);

			tabs.addStateListener(onTabChanged);
		}
		
		private function onSelectionChange(e: SelectionEvent) : void {			
			if (rankings[type].baseOn == "city") {
				

			}			
		}

		private function onTabChanged(e: AWEvent) : void {
			rankingTable.getParent().remove(rankingScroll);
			(tabs.getSelectedComponent() as Container).append(rankingScroll);
			(tabs.getSelectedComponent() as Container).pack();

			changeType();
		}

		private function onChangeRanking(e: AWEvent) : void {
			changeType();
		}

		private function onSearch(e: AWEvent) : void {
			search(txtSearch.getText());
		}

		// This will recalculate the proper ranking type and load the default page
		private function changeType() : void {

			// Here we define which buttons represent what type

			// City ranking
			if (tabs.getSelectedIndex() == 0) {
				if (cityAttackRanking.isSelected()) {
					type = 0;
				} else if (cityDefenseRanking.isSelected()) {
					type = 1;
				} else if (cityLootRanking.isSelected()) {
					type = 2;
				} else {
					type = 3;
				}
			}
			// Player ranking
			else if(tabs.getSelectedIndex() == 1) {
				if (playerAttackRanking.isSelected()) {
					type = 4;
				} else if (playerDefenseRanking.isSelected()) {
					type = 5;
				} else if (playerLootRanking.isSelected()) {
					type = 6;
				} else {
					type = 7;
				}
			}
			// Tribe ranking
			else if (tabs.getSelectedIndex() == 2) {
				if (tribeLevelRanking.isSelected()) {
					type = 8;
				} else if (tribeAttackRanking.isSelected()) {
					type = 9;
				} else if (tribeDefenseRanking.isSelected()) {
					type = 10;
				} else if ( tribeVictoryRanking.isSelected()) {
					type = 11;
				} else if ( tribeVictoryRateRanking.isSelected()) {
					type = 12;
				}
			}
			// Stronghold ranking
			else if (tabs.getSelectedIndex() == 3) {
				if (strongholdLevelRanking.isSelected()) {
					type = 13;
				} else if (strongholdOccupiedRanking.isSelected()) {
					type = 14;
				} else if (strongholdVictoryRateRanking.isSelected()) {
					type = 15;
				}
			}
			

			pagingBar.refreshPage( -1);
		}

		private function search(txt: String) : void {
			Global.mapComm.Ranking.search(loader, txt, type);
		}

		private function loadPage(page: int) : void {
			if (rankings[type].baseOn == "city") {
				Global.mapComm.Ranking.list(loader, Global.gameContainer.selectedCity.id, type, page);
			} else if(rankings[type].baseOn == "player") {
				Global.mapComm.Ranking.list(loader, Constants.playerId, type, page);
			} else if (rankings[type].baseOn == "tribe") {
				Global.mapComm.Ranking.list(loader, Constants.tribeId, type, page);
			} else if (rankings[type].baseOn == "stronghold") {
				Global.mapComm.Ranking.list(loader, 0, type, page);
			}
		}

		private function onLoadRanking(e: Event) : void {
			var data: Object;
			try
			{
				data = loader.getDataAsObject();
			}
			catch (e: Error) {
				InfoDialog.showMessageDialog("Error", "Unable to query ranking. Try again later.");
				return;
			}

			if (data.error != null && data.error != "") {
				InfoDialog.showMessageDialog("Info", data.error);
				return;
			}

			//Paging info
			pagingBar.setData(data);

			if (rankings[type].baseOn == "city")
				onCityRanking(data);
			else if (rankings[type].baseOn == "player")
				onPlayerRanking(data);
			else if (rankings[type].baseOn == "tribe")
				onTribeRanking(data);
			else if (rankings[type].baseOn == "stronghold")
				onStrongholdRanking(data);
		}

		private function onPlayerRanking(data: Object) : void {
			rankingList = new VectorListModel();

			rankingModel = new PropertyTableModel(rankingList,
			["Rank", "Player", rankings[type].name],
			["rank", ".", "value"],
			[null, null, null, null]
			);

			var selectIdx: int = -1;

			for each(var rank: Object in data.rankings) {
				rankingList.append( { "rank": rank.rank, "value": rank.value, "cityId": rank.cityId, "cityName": rank.cityName, "playerName": rank.playerName, "playerId": rank.playerId } );

				if (rank.playerId == Constants.playerId)  {
					selectIdx = rankingList.size() - 1;
				}
			}

			rankingTable.setModel(rankingModel);

			rankingTable.getColumnAt(0).setPreferredWidth(45);
			rankingTable.getColumnAt(1).setPreferredWidth(220);
			rankingTable.getColumnAt(2).setPreferredWidth(150);
			
			rankingTable.getColumnAt(1).setCellFactory(new GeneralTableCellFactory(PlayerLabelCell));

			if (selectIdx > -1) {
				rankingTable.setRowSelectionInterval(selectIdx, selectIdx, true);
			}
		}
		private function onCityRanking(data: Object) : void {
			rankingList = new VectorListModel();

			rankingModel = new PropertyTableModel(rankingList,
			["Rank", "Player", "City", rankings[type].name],
			["rank", ".", ".", "value"],
			[null, null, null]
			);					

			var selectIdx: int = -1;
			
			for each(var rank: Object in data.rankings) {
				rankingList.append( { "rank": rank.rank, "value": rank.value, "cityId": rank.cityId, "cityName": rank.cityName, "playerName": rank.playerName, "playerId": rank.playerId } );

				// If this is our player then we save this index
				if (rank.playerId == Constants.playerId)  {
					selectIdx = rankingList.size() - 1;
				}
			}

			rankingTable.setModel(rankingModel);

			rankingTable.getColumnAt(0).setPreferredWidth(43);
			rankingTable.getColumnAt(1).setPreferredWidth(130);
			rankingTable.getColumnAt(2).setPreferredWidth(130);
			rankingTable.getColumnAt(3).setPreferredWidth(110);
			
			rankingTable.getColumnAt(1).setCellFactory(new GeneralTableCellFactory(PlayerLabelCell));
			rankingTable.getColumnAt(2).setCellFactory(new GeneralTableCellFactory(CityLabelCell));

			// Select player 
			if (selectIdx > -1) {
				rankingTable.setRowSelectionInterval(selectIdx, selectIdx, true);
			}
		}
		private function onTribeRanking(data: Object) : void {
			
			rankingList = new VectorListModel();

			rankingModel = new PropertyTableModel(rankingList,
			["Rank", "Tribe", rankings[type].name],
			["rank", ".", "value"],
			[null, null, null]
			);

			var selectIdx: int = -1;

			for each(var rank: Object in data.rankings) {
				if (!rank || rank.tribeId == null || rank.tribeName == null) continue;
				
				rankingList.append( { "rank": rank.rank, "value": rank.value, "tribeId": rank.tribeId, "tribeName": rank.tribeName } );

				if (rank.tribeId == Constants.tribeId)  {
					selectIdx = rankingList.size() - 1;
				}
			}

			rankingTable.setModel(rankingModel);

			rankingTable.getColumnAt(0).setPreferredWidth(45);
			rankingTable.getColumnAt(1).setPreferredWidth(220);
			rankingTable.getColumnAt(2).setPreferredWidth(150);
			
			rankingTable.getColumnAt(1).setCellFactory(new GeneralTableCellFactory(TribeLabelCell));


			if (selectIdx > -1) {
				rankingTable.setRowSelectionInterval(selectIdx, selectIdx, true);
			}
		}
		
		private function onStrongholdRanking(data: Object) : void {
			
			rankingList = new VectorListModel();

			rankingModel = new PropertyTableModel(rankingList,
			["Rank", "Stronghold", "Tribe", rankings[type].name],
			["rank", ".", ".", "value"],
			[null, null, null, rankings[type].name=="Days Occupied"?new DaysOccupiedRankTranslator():null]
			);

			var selectIdx: int = -1;

			for each(var rank: Object in data.rankings) {
				if (!rank || rank.strongholdId == null || rank.strongholdName == null) continue;
				
				rankingList.append( { "rank": rank.rank, "value": rank.value, "strongholdId": rank.strongholdId, "strongholdName": rank.strongholdName, "tribeId": rank.tribeId, "tribeName": rank.tribeName } );
			}

			rankingTable.setModel(rankingModel);

			rankingTable.getColumnAt(0).setPreferredWidth(45);
			rankingTable.getColumnAt(1).setPreferredWidth(220);
			rankingTable.getColumnAt(2).setPreferredWidth(200);
			rankingTable.getColumnAt(3).setPreferredWidth(150);
			
			rankingTable.getColumnAt(1).setCellFactory(new GeneralTableCellFactory(StrongholdLabelCell));
			rankingTable.getColumnAt(2).setCellFactory(new GeneralTableCellFactory(TribeLabelCell));

			if (selectIdx > -1) {
				rankingTable.setRowSelectionInterval(selectIdx, selectIdx, true);
			}
		}

		public function show(owner:* = null, modal:Boolean = true, onClose: Function = null) :JFrame
		{
			super.showSelf(owner, modal, onClose);
			Global.gameContainer.showFrame(frame);

			pagingBar.refreshPage();

			return frame;
		}

		private function createUI():void {
			title = "Ranking";
			setLayout(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 5));

			rankingTable = new JTable();
			rankingTable.setSelectionMode(JTable.SINGLE_SELECTION);
			rankingTable.setPreferredSize(new IntDimension(435, 350));
			
			rankingScroll = new JScrollPane(rankingTable, JScrollPane.SCROLLBAR_AS_NEEDED, JScrollPane.SCROLLBAR_NEVER);

			cityRanking = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 5));
			cityAttackRanking = new JToggleButton("Attack");
			cityAttackRanking.setSelected(true);
			cityDefenseRanking = new JToggleButton("Defense");
			cityLootRanking = new JToggleButton("Loot");
			cityInfluenceRanking = new JToggleButton("Influence");
			var cityButtonGroup: ButtonGroup = new ButtonGroup();
			cityButtonGroup.appendAll(cityAttackRanking, cityDefenseRanking, cityLootRanking,cityInfluenceRanking);
			var cityButtonGroupHolder: JPanel = new JPanel();
			cityButtonGroupHolder.appendAll(cityAttackRanking, cityDefenseRanking, cityLootRanking,cityInfluenceRanking);
			cityRanking.append(cityButtonGroupHolder);			
			cityRanking.append(rankingScroll);

			playerRanking = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 5));
			playerAttackRanking = new JToggleButton("Attack");
			playerAttackRanking.setSelected(true);
			playerDefenseRanking = new JToggleButton("Defense");
			playerLootRanking = new JToggleButton("Loot");
			playerInfluenceRanking = new JToggleButton("Influence");
			var playerButtonGroup: ButtonGroup = new ButtonGroup();
			playerButtonGroup.appendAll(playerAttackRanking, playerDefenseRanking, playerLootRanking,playerInfluenceRanking);
			var playerButtonGroupHolder: JPanel = new JPanel();
			playerButtonGroupHolder.appendAll(playerAttackRanking, playerDefenseRanking, playerLootRanking,playerInfluenceRanking);
			playerRanking.append(playerButtonGroupHolder);

			tribeRanking = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 5));
			tribeLevelRanking = new JToggleButton("Level");
			tribeLevelRanking.setSelected(true);
			tribeAttackRanking = new JToggleButton("Attack");
			tribeDefenseRanking = new JToggleButton("Defense");
			tribeVictoryRanking = new JToggleButton("Victory");
			tribeVictoryRateRanking = new JToggleButton("Victory Point Rate");
			var tribeButtonGroup: ButtonGroup = new ButtonGroup();
			tribeButtonGroup.appendAll(tribeLevelRanking,tribeAttackRanking,tribeDefenseRanking,tribeVictoryRanking,tribeVictoryRateRanking);
			var tribeButtonGroupHolder: JPanel = new JPanel();
			tribeButtonGroupHolder.appendAll(tribeLevelRanking, tribeAttackRanking, tribeDefenseRanking,tribeVictoryRanking,tribeVictoryRateRanking);
			tribeRanking.appendAll(tribeButtonGroupHolder);
			
			strongholdRanking = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 5));
			strongholdLevelRanking = new JToggleButton("Level");
			strongholdOccupiedRanking = new JToggleButton("Days Occupied");
			strongholdVictoryRateRanking = new JToggleButton("Victory Point Rate");
			strongholdOccupiedRanking.setSelected(true);

			var strongholdButtonGroup: ButtonGroup = new ButtonGroup();
			strongholdButtonGroup.appendAll(strongholdLevelRanking,strongholdOccupiedRanking,strongholdVictoryRateRanking);
			var strongholdButtonGroupHolder: JPanel = new JPanel();
			strongholdButtonGroupHolder.appendAll(strongholdLevelRanking,strongholdOccupiedRanking,strongholdVictoryRateRanking);
			strongholdRanking.appendAll(strongholdButtonGroupHolder);
			
			
			tabs = new JTabbedPane();
			tabs.appendTab(cityRanking, "City");
			tabs.appendTab(playerRanking, "Player");
			tabs.appendTab(tribeRanking, "Tribe");
			tabs.appendTab(strongholdRanking, "Stronghold");

			// Bottom bar
			var pnlFooter: JPanel = new JPanel(new BorderLayout(10));

			// Paging
			pagingBar = new PagingBar(loadPage, false);

			// Search
			var pnlSearch: JPanel = new JPanel();
			pnlSearch.setConstraints("East");

			txtSearch = new JTextField("", 6);
			new SimpleTooltip(txtSearch, "Enter a rank or a name to search for");

			btnSearch = new JButton("Search");
			
			// Updated label
			var lblUpdated: JLabel = new JLabel("Ranking is updated on the hour", null, AsWingConstants.LEFT);
			lblUpdated.setFont(lblUpdated.getFont().changeItalic(true));			
			lblUpdated.setConstraints("South");			

			//component layoution
			pnlSearch.append(txtSearch);
			pnlSearch.append(btnSearch);

			pnlFooter.append(pagingBar);
			pnlFooter.append(pnlSearch);
			pnlFooter.append(lblUpdated);

			append(tabs);
			append(pnlFooter);
		}
	}
}

