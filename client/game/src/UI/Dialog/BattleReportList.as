package src.UI.Dialog
{
    import flash.events.Event;

    import org.aswing.*;
    import org.aswing.border.*;
    import org.aswing.geom.*;

    import src.Comm.GameURLLoader;
    import src.Constants;
    import src.Global;
    import src.UI.Components.BattleReport.BattleReportListTable;
    import src.UI.Components.BattleReport.LocalReportList;
    import src.UI.Components.BattleReport.RemoteReportList;
    import src.UI.Components.SimpleTooltip;
    import src.UI.GameJPanel;

    public class BattleReportList extends GameJPanel
	{
		private var btnMarkAsRead: JButton;
		
		private var pnlLocal: JPanel;
		private var pnlRemote: JPanel;

		private var localReports: LocalReportList;
		private var remoteReports: RemoteReportList;		
		
		private var txtAdminCityList: JTextField;
		private var btnAdminSearch: JButton;

		public function BattleReportList() {
			createUI();
			
			btnMarkAsRead.addActionListener(function(e: Event): void 
			{
				var loader: GameURLLoader = new GameURLLoader();
				loader.addEventListener(Event.COMPLETE, function(e1: Event): void 
				{
					localReports.loadPage(0);
					remoteReports.loadPage(0);
				});				
				Global.mapComm.BattleReport.markAllAsRead(loader);
				localReports.refreshOnClose = true;
			});
			
			btnAdminSearch.addActionListener(onAdminCitySearch);
		}
		
		public function onAdminCitySearch(e: Event) : void {
			localReports.filterPlayerName(txtAdminCityList.getText());
			remoteReports.filterPlayerName(txtAdminCityList.getText());
		}
		
		public function getRefreshOnClose() : Boolean {
			return localReports.refreshOnClose || remoteReports.refreshOnClose;
		}

		public function show(owner:* = null, modal:Boolean = true, onClose: Function = null):JFrame
		{
			super.showSelf(owner, modal, onClose);
			Global.gameContainer.showFrame(frame);
			frame.setResizable(true);
			frame.setTitle("Battle Reports");
            
            localReports.loadInitially();
            remoteReports.loadInitially();
            
			return frame;
		}

		public function createUI() : void {
			setPreferredWidth(600);

			var layout0:SoftBoxLayout = new SoftBoxLayout();
			layout0.setAxis(AsWingConstants.VERTICAL);
			layout0.setGap(10);
			setLayout(layout0);
			
			btnMarkAsRead = new JButton("Mark All as Read");

			pnlLocal = new JPanel();
			pnlLocal.setPreferredSize(new IntDimension(600, 230));
			var border1:TitledBorder = new TitledBorder(null, "Invasion Reports", 1, AsWingConstants.LEFT, 0, 10);
			border1.setColor(new ASColor(0x0, 1));			
			border1.setBeveled(true);			
			pnlLocal.setBorder(border1);
			pnlLocal.setLayout(new BorderLayout());

			localReports = new LocalReportList(BattleReportViewer.REPORT_CITY_LOCAL, [BattleReportListTable.COLUMN_DATE_UNREAD_BOLD, BattleReportListTable.COLUMN_LOCATION]);
			localReports.setConstraints("Center");
			pnlLocal.append(localReports);

			pnlRemote = new JPanel();
			pnlRemote.setPreferredSize(new IntDimension(600, 230));
			var border2:TitledBorder = new TitledBorder(null, "Foreign Reports", 1, AsWingConstants.LEFT, 0, 10);
			border2.setColor(new ASColor(0x0, 1));			
			border2.setBeveled(true);
			pnlRemote.setBorder(border2);
			pnlRemote.setLayout(new BorderLayout());

			remoteReports = new RemoteReportList(BattleReportViewer.REPORT_CITY_FOREIGN, [BattleReportListTable.COLUMN_DATE_UNREAD_BOLD, BattleReportListTable.COLUMN_LOCATION, BattleReportListTable.COLUMN_TROOP_NAME, BattleReportListTable.COLUMN_SIDE]);
			remoteReports.setConstraints("Center");
			pnlRemote.append(remoteReports);
			
			// Admin only feature
			var pnlAdminSearch: JPanel = new JPanel(new FlowLayout(AsWingConstants.RIGHT));	
			txtAdminCityList = new JTextField("", 10);
			new SimpleTooltip(txtAdminCityList, "Type a player name to view their reports");			
			btnAdminSearch = new JButton("Search");											

			// component layout			
			append(AsWingUtils.createPaneToHold(btnMarkAsRead, new FlowLayout()));
			append(pnlLocal);
			append(pnlRemote);
			append(new JLabel("Reports are removed after two weeks.", null, AsWingConstants.LEFT));
			
			if (Constants.admin) {
				pnlAdminSearch.append(txtAdminCityList);
				pnlAdminSearch.append(btnAdminSearch);				
				append(pnlAdminSearch);			
				setPreferredHeight(getPreferredHeight() + 50);
			}
		}
	}

}

