package src.UI.Dialog{

	import flash.events.Event;
	import org.aswing.event.AWEvent;
	import org.aswing.event.SelectionEvent;
	import org.aswing.table.GeneralTableCellFactory;
	import org.aswing.table.PropertyTableModel;
	import src.Comm.GameURLLoader;
	import src.Constants;
	import src.Global;
	import src.UI.Components.SimpleTooltip;
	import src.UI.Components.TableCells.GoToCityTextCell;
	import src.UI.GameJPanel;
	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.geom.*;
	import org.aswing.colorchooser.*;
	import org.aswing.event.TableCellEditEvent;
	import org.aswing.ext.*;

	public class RankingDialog extends GameJPanel {

		private var rankings: Array = [
		{name: "Attack Points", cityBased: true},
		{name: "Defense Points", cityBased: true},
		{name: "Resources Stolen", cityBased: true},
		{name: "Attack Points", cityBased: false},
		{name: "Defense Points", cityBased: false},
		{name: "Resources Stolen", cityBased: false},
		];

		private var loader: GameURLLoader;
		private var page: int = -1;
		private var type: int = 0;
		private var pnlPaging:JPanel;
		private var btnPrevious:JLabelButton;
		private var btnFirst:JLabelButton;
		private var lblPages:JLabel;
		private var btnNext:JLabelButton;

		private var pnlLoading: GameJPanel;

		private var rankingList: VectorListModel;
		private var rankingModel: PropertyTableModel;
		private var rankingTable: JTable;

		private var tabs: JTabbedPane;
		private var cityRanking: JPanel;
		private var cityAttackRanking: JToggleButton;
		private var cityDefenseRanking: JToggleButton;
		private var cityLootRanking: JToggleButton;

		private var playerRanking: JPanel;
		private var playerAttackRanking: JToggleButton;
		private var playerDefenseRanking: JToggleButton;
		private var playerLootRanking: JToggleButton;

		private var txtSearch: JTextField;
		private var btnSearch: JButton;
		
		private var rankingScroll: JScrollPane;

		public function RankingDialog() {
			loader = new GameURLLoader();
			loader.addEventListener(Event.COMPLETE, onLoadRanking);

			createUI();

			// Disables editing the table
			rankingTable.addEventListener(TableCellEditEvent.EDITING_STARTED, function(e: TableCellEditEvent) : void {
				rankingTable.getCellEditor().stopCellEditing();
			});
			
			// Any special row selection stuff goes here
			rankingTable.addEventListener(SelectionEvent.COLUMN_SELECTION_CHANGED, onSelectionChange);
			rankingTable.addEventListener(SelectionEvent.ROW_SELECTION_CHANGED, onSelectionChange);
		
			// Paging buttons
			btnFirst.addActionListener(function() : void {
				loadPage(1);
			});

			btnNext.addActionListener(function() : void {
				loadPage(page + 1);
			});

			btnPrevious.addActionListener(function() : void{
				loadPage(page - 1);
			});

			// Tooltips
			new SimpleTooltip(cityAttackRanking, "Sort by attack points");
			new SimpleTooltip(playerAttackRanking, "Sort by attack points");
			new SimpleTooltip(cityDefenseRanking, "Sort by defense points");
			new SimpleTooltip(playerDefenseRanking, "Sort by attack points");
			new SimpleTooltip(cityLootRanking, "Sort by total loot stolen");
			new SimpleTooltip(playerLootRanking, "Sort by total loot stolen");
			
			// Handle different buttons being pressed
			cityAttackRanking.addActionListener(onChangeRanking);
			cityDefenseRanking.addActionListener(onChangeRanking);
			cityLootRanking.addActionListener(onChangeRanking);

			playerAttackRanking.addActionListener(onChangeRanking);
			playerDefenseRanking.addActionListener(onChangeRanking);
			playerLootRanking.addActionListener(onChangeRanking);

			btnSearch.addActionListener(onSearch);

			tabs.addStateListener(onTabChanged);
		}
		
		private function onSelectionChange(e: SelectionEvent) : void {			
			if (rankings[type].cityBased) {
				

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
				} else {
					type = 2;
				}
			}
			// Player ranking
			else {
				if (playerAttackRanking.isSelected()) {
					type = 3;
				} else if (playerDefenseRanking.isSelected()) {
					type = 4;
				} else {
					type = 5;
				}
			}

			loadPage(-1);
		}

		private function search(txt: String) : void {
			pnlLoading = InfoDialog.showMessageDialog("Loading", "Searching...", null, null, true, false, 0);

			Global.mapComm.Ranking.search(loader, txt, type);
		}

		private function loadPage(page: int) : void {
			pnlLoading = InfoDialog.showMessageDialog("Loading", "Loading ranking...", null, null, true, false, 0);

			if (rankings[type].cityBased) {
				Global.mapComm.Ranking.list(loader, Global.gameContainer.selectedCity.id, type, page);
			} else {
				Global.mapComm.Ranking.list(loader, Constants.playerId, type, page);
			}
		}

		private function onLoadRanking(e: Event) : void {
			pnlLoading.getFrame().dispose();

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
			this.page = data.page;
			btnFirst.setVisible(page > 1);
			btnPrevious.setVisible(page > 1);
			btnNext.setVisible(page < data.pages);
			lblPages.setText(data.page + " of " + data.pages);

			if (rankings[type].cityBased)
			onCityRanking(data);
			else
			onPlayerRanking(data);
		}

		private function onPlayerRanking(data: Object) : void {
			rankingList = new VectorListModel();

			rankingModel = new PropertyTableModel(rankingList,
			["Rank", "Player", rankings[type].name],
			["rank", "playerName", "value"],
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

			if (selectIdx > -1) {
				rankingTable.setRowSelectionInterval(selectIdx, selectIdx, true);
			}
		}
		private function onCityRanking(data: Object) : void {
			rankingList = new VectorListModel();

			rankingModel = new PropertyTableModel(rankingList,
			["Rank", "Player", "City", rankings[type].name, ""],
			["rank", "playerName", "cityName", "value", "cityId"],
			[null, null, null, null]
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
			rankingTable.getColumnAt(1).setPreferredWidth(120);
			rankingTable.getColumnAt(2).setPreferredWidth(120);
			rankingTable.getColumnAt(3).setPreferredWidth(110);
			rankingTable.getColumnAt(4).setPreferredWidth(20);
			
			rankingTable.getColumnAt(4).setCellFactory(new GeneralTableCellFactory(GoToCityTextCell));

			// Select player 
			if (selectIdx > -1) {
				rankingTable.setRowSelectionInterval(selectIdx, selectIdx, true);
			}
		}

		public function show(owner:* = null, modal:Boolean = true, onClose: Function = null) :JFrame
		{
			super.showSelf(owner, modal, onClose);
			Global.gameContainer.showFrame(frame);

			loadPage(page);

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
			var cityButtonGroup: ButtonGroup = new ButtonGroup();
			cityButtonGroup.appendAll(cityAttackRanking, cityDefenseRanking, cityLootRanking);
			var cityButtonGroupHolder: JPanel = new JPanel();
			cityButtonGroupHolder.appendAll(cityAttackRanking, cityDefenseRanking, cityLootRanking);
			cityRanking.append(cityButtonGroupHolder);			
			cityRanking.append(rankingScroll);

			playerRanking = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 5));
			playerAttackRanking = new JToggleButton("Attack");
			playerAttackRanking.setSelected(true);
			playerDefenseRanking = new JToggleButton("Defense");
			playerLootRanking = new JToggleButton("Loot");
			var playerButtonGroup: ButtonGroup = new ButtonGroup();
			playerButtonGroup.appendAll(playerAttackRanking, playerDefenseRanking, playerLootRanking);
			var playerButtonGroupHolder: JPanel = new JPanel();
			playerButtonGroupHolder.appendAll(playerAttackRanking, playerDefenseRanking, playerLootRanking);
			playerRanking.append(playerButtonGroupHolder);

			tabs = new JTabbedPane();
			tabs.appendTab(cityRanking, "City");
			tabs.appendTab(playerRanking, "Player");

			// Bottom bar
			var pnlFooter: JPanel = new JPanel(new BorderLayout(10));

			// Paging
			pnlPaging = new JPanel();
			pnlPaging.setConstraints("West");

			btnFirst = new JLabelButton("<< First");
			btnPrevious = new JLabelButton("< Previous");
			btnNext = new JLabelButton("Next >");

			lblPages = new JLabel();

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
			pnlPaging.append(btnFirst);
			pnlPaging.append(btnPrevious);
			pnlPaging.append(lblPages);
			pnlPaging.append(btnNext);

			pnlSearch.append(txtSearch);
			pnlSearch.append(btnSearch);

			pnlFooter.append(pnlPaging);
			pnlFooter.append(pnlSearch);
			pnlFooter.append(lblUpdated);

			append(tabs);
			append(pnlFooter);
		}
	}
}

