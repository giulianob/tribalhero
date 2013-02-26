package src.UI.Dialog 
{
	import fl.lang.*;
	import flash.events.*;
	import flash.utils.*;
	import mx.utils.*;
	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.colorchooser.*;
	import org.aswing.event.*;
	import org.aswing.ext.*;
	import org.aswing.geom.*;
	import org.aswing.table.*;
	import src.*;
	import src.Objects.*;
	import src.Objects.Effects.*;
	import src.Objects.Process.*;
	import src.Objects.Stronghold.*;
	import src.UI.*;
	import src.UI.Components.*;
	import src.UI.Components.BattleReport.*;
	import src.UI.Components.TableCells.*;
	import src.UI.Components.Tribe.*;
	import src.UI.LookAndFeel.*;
	import src.UI.Tooltips.*;
	import src.Util.*;
	
	public class TribeProfileDialog extends GameJPanel
	{
		private var profileData: * ;
		
		private var pnlHeader: JPanel;
		private var pnlInfoContainer: JPanel;
		
		private var messageBoard: MessageBoard;
		
		private var pnlTabs: JTabbedPane;
		private var pnlInfoTabs: JTabbedPane;
		
		private var updateTimer: Timer;
		private var pnlStrongholds:JPanel;
		private var remoteReports:RemoteReportList;
		private var localReports:LocalReportList;
		private var strongholdTab:JPanel;
		private var messageBoardTab:JPanel;
		private var pnlOngoingAttacks:JPanel;
		
		public function TribeProfileDialog(profileData: *) 
		{
			this.profileData = profileData;
			
			createUI();				
			
			// Refreshes the general tribe info
			updateTimer = new Timer(120 * 1000);
			updateTimer.addEventListener(TimerEvent.TIMER, function(e: Event = null): void {
				update();
			});			
			updateTimer.start();
			
			pnlTabs.addStateListener(function (e: InteractiveEvent): void {
				if (pnlTabs.getSelectedComponent() == strongholdTab) {
					localReports.loadInitially();
					remoteReports.loadInitially();
				}
				else if (pnlTabs.getSelectedComponent() == messageBoardTab) {
					messageBoard.loadInitially();
				}
			});
		}
		
		private function dispose():void {
			updateTimer.stop();
		}
		
		public function update(): void {
			Global.mapComm.Tribe.viewTribeProfile(profileData.tribeId, function(newProfileData: *): void {
				if (!newProfileData) {
					return;
				}
				
				profileData = newProfileData;
				createInfoTab();
				createStrongholdList();
				createOngoingAttacksList();
			});
		}
		
		public function show(owner:* = null, modal:Boolean = true, onClose: Function = null):JFrame 
		{
			super.showSelf(owner, modal, onClose, dispose);
			Global.gameContainer.closeAllFramesByType(TribeProfileDialog);
			Global.gameContainer.showFrame(frame);
			return frame;
		}
		
		private function createUI():void {
			setPreferredSize(new IntDimension(Math.min(1025, Constants.screenW - GameJImagePanelBackground.getFrameWidth()) , Math.max(375, Constants.screenH - GameJImagePanelBackground.getFrameHeight())));
			title = "Tribe Profile - " + profileData.tribeName;
			setLayout(new BorderLayout(10, 10));
			
			// Header panel
			pnlHeader = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS));
			pnlHeader.setConstraints("North");		
			
			// Tab panel
			pnlTabs = new JTabbedPane();
			pnlTabs.setPreferredSize(new IntDimension(375, 350));
			pnlTabs.setConstraints("Center");
						
			// Append tabs			
			pnlTabs.appendTab(createInfoTab(), "Info");
			
			messageBoardTab = createMessageBoardTab();
			pnlTabs.appendTab(messageBoardTab, "Message Board");
			
			strongholdTab = createStrongholdTab();
			pnlTabs.appendTab(strongholdTab, StringHelper.localize("STR_STRONGHOLDS"));
			
			// Append main panels
			appendAll(pnlHeader, pnlTabs);
		}
	
		private function createMessageBoardTab(): JPanel {		
			messageBoard = new MessageBoard();					
			return messageBoard;
		}
		
		private function simpleLabelMaker(text: String, tooltip:String, icon:Icon = null):JLabel
		{
			var label:JLabel = new JLabel(text, icon);
			
			label.setIconTextGap(0);
			label.setHorizontalTextPosition(AsWingConstants.RIGHT);
			label.setHorizontalAlignment(AsWingConstants.LEFT);
			
			new SimpleTooltip(label, tooltip);
			
			return label;
		}

		private function createOngoingAttackItem(stronghold : * ): JPanel {
			var pnl: JPanel = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS,0));
			var pnlTop: JPanel = new JPanel(new BorderLayout(5, 0));
			var pnlBottom: JPanel = new JPanel(new BorderLayout(5, 0));
			
			var lblName : StrongholdLabel = new StrongholdLabel(stronghold.id, false, stronghold.name);
			lblName.setHorizontalAlignment(AsWingConstants.LEFT);
			lblName.setVerticalAlignment(AsWingConstants.TOP);
			
			var pnlNameStatus: JPanel = new JPanel(new SoftBoxLayout(SoftBoxLayout.X_AXIS, 5));			
			pnlNameStatus.setPreferredHeight(25);
			pnlNameStatus.append(lblName);				
			
			var grid: JPanel = new JPanel(new FlowLayout(AsWingConstants.LEFT, 10, 0, false));
			grid.append(simpleLabelMaker(StringHelper.localize("STR_LEVEL_VALUE", stronghold.lvl), StringHelper.localize("STR_LEVEL"), new AssetIcon(new ICON_UPGRADE())));
			
			if (stronghold.tribeId == 0) {
				grid.append(simpleLabelMaker(StringHelper.localize("STR_NEUTRAL"), StringHelper.localize("STR_NEUTRAL"), new AssetIcon(new ICON_SHIELD())));
			}			
			else {
				var tribeLabel: TribeLabel = new TribeLabel(stronghold.tribeId, stronghold.tribeName)
				tribeLabel.setIcon(new AssetIcon(new ICON_SHIELD()));
				grid.append(tribeLabel);
			}
			
			pnlTop.setPreferredHeight(25);
			pnlTop.append(pnlNameStatus, "Center");			
			
			pnlBottom.append(grid, "Center");
			
			pnl.setPreferredWidth(400);
			pnl.appendAll(pnlTop, pnlBottom);
			
			return pnl;
		}		
		
		private function createStrongholdItem(stronghold : * ): JPanel {
			var pnl: JPanel = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS,0));
			var pnlTop: JPanel = new JPanel(new BorderLayout(5, 0));
			var pnlBottom: JPanel = new JPanel(new BorderLayout(5, 0));
			
			var lblName : StrongholdLabel = new StrongholdLabel(stronghold.id, stronghold.name);
			lblName.setHorizontalAlignment(AsWingConstants.LEFT);
			lblName.setVerticalAlignment(AsWingConstants.TOP);
			
			var pnlNameStatus: JPanel = new JPanel(new SoftBoxLayout(SoftBoxLayout.X_AXIS, 5));			
			pnlNameStatus.setPreferredHeight(25);
			pnlNameStatus.append(lblName);				
			pnlNameStatus.append(Stronghold.getBattleStateString(stronghold, 2, 30));			
			
			var grid: JPanel = new JPanel(new FlowLayout(AsWingConstants.LEFT, 10, 0, false));
			grid.append(simpleLabelMaker(StringHelper.localize("STR_LEVEL_VALUE", stronghold.lvl), StringHelper.localize("STR_LEVEL"), new AssetIcon(new ICON_UPGRADE())));
			grid.append(simpleLabelMaker(StringHelper.localize("STR_PER_DAY_RATE", Util.roundNumber(stronghold.victoryPointRate)), StringHelper.localize("STR_VP_RATE"), new AssetIcon(new ICON_STAR())));
			var timediff :int = Global.map.getServerTime() - stronghold.dateOccupied;
			grid.append(simpleLabelMaker(Util.niceDays(timediff), StringHelper.localize("STR_DAYS_OCCUPIED"), new AssetIcon(new ICON_SHIELD())));
			
			var lblTroop: JLabel = new JLabel(StringHelper.localize("STR_UNIT_SINGULAR_PLURAL", stronghold.upkeep));
			lblTroop.setHorizontalAlignment(AsWingConstants.RIGHT);

			var pnlGate: JPanel = new JPanel(new SoftBoxLayout(SoftBoxLayout.X_AXIS, 0, AsWingConstants.RIGHT));									
			pnlGate.setPreferredHeight(25);
			var lblGate: JLabel = new JLabel(StringHelper.localize("STR_GATE"), null, AsWingConstants.LEFT);
			lblGate.setVerticalAlignment(AsWingConstants.TOP);
			pnlGate.append(lblGate);
			
			if (stronghold.battleState == Stronghold.BATTLE_STATE_NONE && stronghold.gate < Formula.getGateLimit(stronghold.lvl) && Constants.tribeRank <= 1) {
				var btnGateRepair: JLabelButton = new JLabelButton(Stronghold.gateToString(stronghold.lvl, stronghold.gate), null, AsWingConstants.LEFT);
				btnGateRepair.useHandCursor = true;
				btnGateRepair.addEventListener(MouseEvent.CLICK, function(e: Event): void {
					Global.mapComm.Stronghold.repairStrongholdGate(stronghold.id, function(): void {
						update();
					});
				});
				var tooltip: SimpleTooltip = new SimpleTooltip(btnGateRepair, StringHelper.localize("STRONGHOLD_REPAIR_GATE_ACTION"));
				tooltip.append(new ResourcesPanel(Formula.getGateRepairCost(stronghold.lvl, stronghold.gate), profileData.resources, true, false));				
				btnGateRepair.setVerticalAlignment(AsWingConstants.TOP);
				pnlGate.append(btnGateRepair);
			}						
			else {
				var lblGateHealth: JLabel = new JLabel(Stronghold.gateToString(stronghold.lvl, stronghold.gate), null, AsWingConstants.LEFT);
				lblGateHealth.setVerticalAlignment(AsWingConstants.TOP);
				pnlGate.append(lblGateHealth);
			}
						
			pnlTop.setPreferredHeight(25);
			pnlTop.append(pnlNameStatus, "Center");
			pnlTop.append(pnlGate, "East");
			
			pnlBottom.append(grid, "Center");
			pnlBottom.append(lblTroop, "East");
			
			pnl.setPreferredWidth(400);
			pnl.appendAll(pnlTop, pnlBottom);
			
			return pnl;
		}
		
		private function createStrongholdTab(): JPanel {
			// Strongholds List
			var pnl: JPanel = createStrongholdList();			
			var tabTroops: JTabbedPane = new JTabbedPane();
			tabTroops.appendTab(Util.createTopAlignedScrollPane(pnl), StringHelper.localize("STR_STRONGHOLDS_UNDER_COMMAND"));

			//  Ongoing Attack Tab
			pnl = createOngoingAttacksList();
			var tabOnGoing: JTabbedPane = new JTabbedPane();
			tabOnGoing.appendTab(Util.createTopAlignedScrollPane(pnl), StringHelper.localize("STR_ONGOING_ATTACKS"));
			tabOnGoing.setPreferredHeight(150);

			// Report Tab
			pnl = createReportsPanels();
			var tabReports: JTabbedPane = new JTabbedPane();
			tabReports.appendTab(pnl, StringHelper.localize("STR_REPORTS"));
			
			// Troop + Ongoing
			var pnlLeft : JPanel = new JPanel(new BorderLayout(10,10));
			tabTroops.setConstraints("Center");
			tabOnGoing.setConstraints("South");
			pnlLeft.appendAll(tabTroops, tabOnGoing);
			
			// Main tab
			pnl = new JPanel(new BorderLayout(10, 10));
			pnlLeft.setConstraints("West");
			pnlLeft.setPreferredWidth(Math.max(150, getPreferredWidth() / 2.5));
			tabReports.setConstraints("Center");
			pnl.appendAll(pnlLeft, tabReports);
			
			return pnl;
		}
		
		private function createOngoingAttacksList(): JPanel
		{
			if (!pnlOngoingAttacks) {
				pnlOngoingAttacks = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 15));
			}
			else {
				pnlOngoingAttacks.removeAll();
			}
			
			for each (var stronghold: * in profileData.openStrongholds) {
				pnlOngoingAttacks.append(createOngoingAttackItem(stronghold));
			}
			
			if (profileData.openStrongholds.length == 0) {
				pnlOngoingAttacks.append(new JLabel(StringHelper.localize("TRIBE_NO_STRONGHOLDS_OPEN"), null, AsWingConstants.LEFT));
			}
			
			return pnlOngoingAttacks;			
		}
		
		private function createReportsPanels(): JPanel 
		{			
			var localReportBorder:TitledBorder = new TitledBorder(null, "Invasion Reports", 1, AsWingConstants.LEFT, 0, 10);				
			localReportBorder.setBeveled(true);

			localReports = new LocalReportList(BattleReportViewer.REPORT_TRIBE_LOCAL, [BattleReportListTable.COLUMN_DATE, BattleReportListTable.COLUMN_LOCATION, BattleReportListTable.COLUMN_ATTACK_TRIBES], null);
			localReports.setBorder(localReportBorder);

			var pnlRemote: JPanel = new JPanel();			
			var remoteReportBorder:TitledBorder = new TitledBorder(null, "Foreign Reports", 1, AsWingConstants.LEFT, 0, 10);
			remoteReportBorder.setColor(new ASColor(0x0, 1));			
			remoteReportBorder.setBeveled(true);

			remoteReports = new RemoteReportList(BattleReportViewer.REPORT_TRIBE_FOREIGN, [BattleReportListTable.COLUMN_DATE, BattleReportListTable.COLUMN_LOCATION, BattleReportListTable.COLUMN_DEFENSE_TRIBES], null);
			remoteReports.setBorder(remoteReportBorder);
			
			var pnl: JPanel = new JPanel(new GridLayout(2, 1, 5));
			pnl.appendAll(localReports, remoteReports);
			
			return pnl;
		}
		
		private function createStrongholdList(): JPanel 
		{
			if (!pnlStrongholds) {
				pnlStrongholds = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 15));
			}
			else {
				pnlStrongholds.removeAll();
			}
			
			for each (var stronghold: * in profileData.strongholds) {
				pnlStrongholds.append(createStrongholdItem(stronghold));
			}
			
			if (profileData.strongholds.length == 0) {
				pnlStrongholds.append(new JLabel(StringHelper.localize("TRIBE_NO_STRONGHOLDS_OWNED"), null, AsWingConstants.LEFT));
			}
			
			return pnlStrongholds;
		}
		
		private function createIncomingPanelItem(incoming: *): JPanel {
			var pnlContainer: JPanel = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 0));
			
			var pnlHeader: JPanel = new JPanel(new FlowLayout(AsWingConstants.LEFT, 0, 0, false));
			
			pnlHeader.append(new RichLabel(StringHelper.localize("TRIBE_INCOMING_ATK", RichLabel.getHtmlForLocation(incoming.target), RichLabel.getHtmlForLocation(incoming.source)), 2, 50));
						
			var lblCountdown: CountDownLabel = new CountDownLabel(incoming.endTime, StringHelper.localize("STR_BATTLE_IN_PROGRESS"));
			
			pnlContainer.appendAll(pnlHeader, lblCountdown);
			
			return pnlContainer;
		}
		
		private function createAssignmentItem(assignment: *): JPanel {
			var pnlContainer: JPanel = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 0));
			
			var pnlName: RichLabel = new RichLabel(StringUtil.substitute(Locale.loadString(assignment.isAttack?"ASSIGNMENT_ATK":"ASSIGNMENT_DEF"),RichLabel.getHtmlForLocation(assignment.target)));
			var pnlStats: JPanel = new JPanel(new SoftBoxLayout(AsWingConstants.RIGHT, 5, AsWingConstants.RIGHT));
			pnlStats.appendAll(
				new JLabel(assignment.troopCount, new AssetIcon(assignment.isAttack?new ICON_SINGLE_SWORD:new ICON_SHIELD), AsWingConstants.RIGHT),
				new CountDownLabel(assignment.endTime, "Troops Dispatched")
			);
			pnlStats.setConstraints("East");
			
			var pnlHeader: JPanel = new JPanel(new BorderLayout(10));
			pnlHeader.appendAll(pnlName, pnlStats);
			
			var pnlBottom: JPanel = new JPanel(new BorderLayout(5));
			
			var btnJoin: JLabelButton = new JLabelButton("Join", null, AsWingConstants.RIGHT);
			btnJoin.setConstraints("East");
			
			var btnDetails: JLabelButton = new JLabelButton("Details", null, AsWingConstants.LEFT);

			var lblDescription: JLabel = new JLabel(StringHelper.truncate(assignment.description,75), null, AsWingConstants.LEFT);
			lblDescription.setConstraints("Center");
			if (assignment.description != "") 
				new SimpleTooltip(lblDescription, assignment.description);
			
			var pnlButtons: JPanel = new JPanel(new FlowLayout());
			pnlButtons.appendAll(btnDetails, btnJoin);
			pnlButtons.setConstraints("East");
			
			pnlBottom.appendAll(lblDescription, pnlButtons);
			
			pnlContainer.appendAll(pnlHeader, pnlBottom);
			
			btnDetails.addActionListener(function(e: Event): void {
				var info: AssignmentInfoDialog = new AssignmentInfoDialog(assignment);
				info.show();
			});
			
			btnJoin.addActionListener(function(e: Event): void {
				var join: AssignmentJoinProcess = new AssignmentJoinProcess(Global.gameContainer.selectedCity, assignment);
				join.execute();
			});			
						
			return pnlContainer;
		}		
		
		private function createAssignmentTab() : Container {
			var btnCreate: JButton = new JButton("Create");
			
			var pnlFooter: JPanel = new JPanel(new FlowLayout(AsWingConstants.LEFT));
			pnlFooter.setConstraints("South");			
			
			// Set up tab
			var pnlAssignmentHolder: JPanel = new JPanel(new BorderLayout(0, 5));
			var pnlAssignments: JPanel = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 10));
			
			for each (var assignment: * in profileData.assignments) {
				pnlAssignments.append(createAssignmentItem(assignment));
			}
			
			var scrollAssignment: JScrollPane = new JScrollPane(new JViewport(pnlAssignments, true), JScrollPane.SCROLLBAR_ALWAYS, JScrollPane.SCROLLBAR_NEVER);
			(scrollAssignment.getViewport() as JViewport).setVerticalAlignment(AsWingConstants.TOP);
			scrollAssignment.setConstraints("Center");
			
			pnlAssignmentHolder.appendAll(scrollAssignment, pnlFooter);
			
			if (Constants.tribeRank <= 1) {
				pnlFooter.append(btnCreate);
			}
			
			var menu : JPopupMenu = new JPopupMenu();
			menu.addMenuItem("Offensive Assignment").addActionListener(function(e: Event): void {
				var assignmentCreate: AtkAssignmentCreateProcess = new AtkAssignmentCreateProcess(Global.gameContainer.selectedCity);
				assignmentCreate.execute();
			});
			menu.addMenuItem("Defensive Assignment").addActionListener(function(e: Event): void {
				var assignmentCreate: DefAssignmentCreateProcess = new DefAssignmentCreateProcess(Global.gameContainer.selectedCity);
				assignmentCreate.execute();
			});

			btnCreate.addActionListener(function(e: Event): void {
				menu.show(btnCreate, 0, btnCreate.height);
			});

			new SimpleTooltip(btnCreate, "An assignment is an organized attack/defense used by the tribe to dispatch troops automatically at different times, so all of them can start the battle at the same time regardless of the distance/speed.");
			
			return pnlAssignmentHolder;
		}
		
		private function createMembersTab() : Container {
			var modelMembers: VectorListModel = new VectorListModel(profileData.members);
			var tableMembers: JTable = new JTable(new PropertyTableModel(
				modelMembers, 
				["Player", "Rank", "Last Seen", ""],
				[".", "rank", "date", "."],
				[null, new TribeRankTranslator(), null]
			));			
			tableMembers.addEventListener(TableCellEditEvent.EDITING_STARTED, function(e: TableCellEditEvent) : void {
				tableMembers.getCellEditor().cancelCellEditing();
			});			
			tableMembers.setRowSelectionAllowed(false);
			tableMembers.setAutoResizeMode(JTable.AUTO_RESIZE_OFF);
			tableMembers.getColumnAt(0).setPreferredWidth(145);
			tableMembers.getColumnAt(0).setCellFactory(new GeneralTableCellFactory(PlayerLabelCell));
			tableMembers.getColumnAt(1).setPreferredWidth(100);
			tableMembers.getColumnAt(2).setPreferredWidth(100);
			tableMembers.getColumnAt(3).setCellFactory(new GeneralTableCellFactory(TribeMemberActionCell));
			tableMembers.getColumnAt(3).setPreferredWidth(70);
			
			var scrollMembers: JScrollPane = new JScrollPane(tableMembers, JScrollPane.SCROLLBAR_ALWAYS, JScrollPane.SCROLLBAR_NEVER);
			
			return scrollMembers;
		}
		
		private function createIncomingAttackTab(): Container {
			var pnlIncomingAttacks: JPanel = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 10));
			
			for each (var incoming: * in profileData.incomingAttacks) {
				pnlIncomingAttacks.append(createIncomingPanelItem(incoming));
			}
			
			var scrollIncomingAttacks: JScrollPane = new JScrollPane(new JViewport(pnlIncomingAttacks, true), JScrollPane.SCROLLBAR_ALWAYS, JScrollPane.SCROLLBAR_NEVER);
			(scrollIncomingAttacks.getViewport() as JViewport).setVerticalAlignment(AsWingConstants.TOP);
			
			return scrollIncomingAttacks;			
		}
		
		private function createInfoTab(): Container {
			// Clear out container if it already exists instead of recreating it
			if (!pnlInfoContainer)
				pnlInfoContainer = new JPanel(new BorderLayout(10, 10));			
			else
				pnlInfoContainer.removeAll();
			
			var btnUpgrade: JLabelButton = new JLabelButton("Upgrade");
			var btnDonate: JLabelButton = new JLabelButton("Contribute");
			var btnInvite: JLabelButton = new JLabelButton("Invite");
			var btnSetDescription: JLabelButton = new JLabelButton("Set Announcement");
			var btnDismantle: JLabelButton = new JLabelButton("Dismantle");
			var btnLeave: JLabelButton = new JLabelButton("Leave");
			var btnTransfer: JLabelButton = new JLabelButton("Transfer Tribe");
			
			var pnlActions: JPanel = new JPanel(new FlowLayout(AsWingConstants.LEFT, 10, 0, false));				
			
			// Show correct buttons depending on rank
			switch (Constants.tribeRank) {
				case 0: 
					pnlActions.appendAll(btnSetDescription, btnInvite, btnDonate, btnUpgrade, btnDismantle, btnTransfer);
					break;
				case 1:
					pnlActions.appendAll(btnInvite, btnDonate, btnUpgrade, btnLeave);
					break;
				default:
					pnlActions.appendAll(btnDonate, btnUpgrade, btnLeave);
					break;
			}
			
			// upgrade btn
			var upgradeTooltip: TribeUpgradeTooltip = new TribeUpgradeTooltip(profileData.tribeLevel, profileData.resources);
			upgradeTooltip.bind(btnUpgrade);
			btnUpgrade.addActionListener(function(e: Event): void {
				Global.mapComm.Tribe.upgrade();
			});
			btnUpgrade.setEnabled(Constants.tribeRank == 0);
			btnUpgrade.mouseEnabled = true;
			
			// description
			var description: String = profileData.description == "" ? "The tribe chief hasn't set an announcement yet" : profileData.description;
			var lblDescription: MultilineLabel = new MultilineLabel(description);
			GameLookAndFeel.changeClass(lblDescription, "Message");
			
			var scrollDescription: JScrollPane = new JScrollPane(lblDescription);
			
			// Side tab browser
			var lastActiveInfoTab: int = 0;
			if (pnlInfoTabs)
				lastActiveInfoTab = pnlInfoTabs.getSelectedIndex();
				
			pnlInfoTabs = new JTabbedPane();
					
			// Put it all together
			var pnlEast: JPanel = new JPanel(new BorderLayout(0, 5));
			pnlInfoTabs.setConstraints("Center");
			pnlEast.appendAll(pnlInfoTabs);
			
			pnlInfoTabs.appendTab(createMembersTab(), "Members (" + profileData.members.length + ")");
			pnlInfoTabs.appendTab(createIncomingAttackTab(), "Invasions (" + profileData.incomingAttacks.length + ")");
			pnlInfoTabs.appendTab(createAssignmentTab(), "Assignments (" + profileData.assignments.length + ")");

			pnlInfoTabs.setSelectedIndex(lastActiveInfoTab);
			
			var pnlWest: JPanel = new JPanel(new BorderLayout(0, 5));
			pnlActions.setConstraints("North");
			scrollDescription.setConstraints("Center");
			
			pnlWest.appendAll(pnlActions, scrollDescription);
				
			pnlWest.setConstraints("Center");
			pnlEast.setConstraints("East");
			
			pnlInfoContainer.appendAll(pnlWest, pnlEast);		
			
			// Button handlers
			btnSetDescription.addActionListener(function(e: Event = null): void {			
				var pnl: JPanel = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 5));
				var txtDescription: JTextArea = new JTextArea(profileData.description, 15, 10);
				txtDescription.setMaxChars(3000);
				GameLookAndFeel.changeClass(txtDescription, "Message");
				var scrollDescription: JScrollPane = new JScrollPane(txtDescription, JScrollPane.SCROLLBAR_AS_NEEDED, JScrollPane.SCROLLBAR_AS_NEEDED);			
			
				pnl.appendAll(new JLabel("Set a message to appears on the tribe profile. This will only be visible to your tribe members.", null, AsWingConstants.LEFT), scrollDescription);
				InfoDialog.showMessageDialog("Say something to your tribe", pnl, function(result: * ): void {
					if (result == JOptionPane.CANCEL || result == JOptionPane.CLOSE)
						return;
						
					Global.mapComm.Tribe.setTribeDescription(txtDescription.getText());					
				});
			});
			
			btnInvite.addActionListener(function(e: Event): void {
				var pnl: JPanel = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS));			
				var txtPlayerName: JTextField = new AutoCompleteTextField(Global.mapComm.General.autoCompletePlayer);
				pnl.appendAll(new JLabel("Type in the name of the player you want to invite", null, AsWingConstants.LEFT), txtPlayerName);				
				
				var invitePlayerName: InfoDialog = InfoDialog.showMessageDialog("Invite a new tribesman", pnl, function(response: * ) : void {
                    if (response != JOptionPane.OK) { return; }
					if (txtPlayerName.getLength() > 0)
						Global.mapComm.Tribe.invitePlayer(txtPlayerName.getText());
				});				
			});
			
			btnDismantle.addActionListener(function(e: Event): void {
				InfoDialog.showInputDialog("Dismantle tribe", "If you really want to dismantle your tribe then type 'delete' below and click ok.", function(input: *) : void {
					if (input == "'delete'" || input == "delete")
						Global.mapComm.Tribe.dismantle();
				});				
			});			
			
			btnTransfer.addActionListener(function(e: Event): void {
				InfoDialog.showInputDialog("Transfer tribe", "Tribes can only be transferred to players that are already part of the tribe. Type in the player name you wish to give this tribe to. You will be demoted to Elder and the player promoted to Owner.", function(input: * ) : void {
					
					if (input == null) {
						return;
					}
					
					InfoDialog.showMessageDialog("Transfer Tribe", StringUtil.substitute("Transfer control to {0}? You cannot regain control of the tribe unless it is transferred back to you.", input), function(result: int): void {
						if (result == JOptionPane.YES)
							Global.mapComm.Tribe.transfer(input);						
					}, null, true, true, JOptionPane.YES | JOptionPane.NO);					
				});				
			});					
			
			btnLeave.addActionListener(function(e: Event): void {
				InfoDialog.showMessageDialog("Leave tribe", "Do you really want to leave the tribe?", function(result: *) : void {
					if (result == JOptionPane.YES)
						Global.mapComm.Tribe.leave();
				}, null, true, true, JOptionPane.YES | JOptionPane.NO);				
			});		
			
			btnDonate.addActionListener(function(e: Event): void {
				InfoDialog.showMessageDialog("Contribute to tribe", "Use a Trading Post to contribute resources.");
			});
			
			// First row of header panel which contains player name + ranking
			var pnlHeaderFirstRow: JPanel = new JPanel(new BorderLayout(5));
			
			var lblTribeName: JLabel = new JLabel(profileData.tribeName + " (Level " + profileData.tribeLevel + ")", null, AsWingConstants.LEFT);				
			GameLookAndFeel.changeClass(lblTribeName, "darkHeader");			
			
			var lblEstablished: JLabel = new JLabel(StringHelper.localize("STR_ESTABLISHED_WITH_TIME", Util.niceDays(Global.map.getServerTime() - profileData.created)));
			
			var pnlHeaderTitle: JPanel = new JPanel(new FlowLayout(AsWingConstants.LEFT, 5, 0, false));
			pnlHeaderTitle.setConstraints("Center");
			pnlHeaderTitle.appendAll(lblTribeName, lblEstablished);
			
			var pnlResources: JPanel = new JPanel(new FlowLayout(AsWingConstants.RIGHT, 10, 0, false));
			var lblVictoryPoint: JLabel = new JLabel(profileData.victoryPoint.toFixed(1),  new AssetIcon(new ICON_STAR()));
			new SimpleTooltip(lblVictoryPoint, StringHelper.localize("STR_VICTORY_POINT"));
			lblVictoryPoint.setIconTextGap(0);
						
			pnlResources.setConstraints("East");
			pnlResources.append(lblVictoryPoint);
			pnlResources.append(new SimpleResourcesPanel(profileData.resources, false));
			pnlHeaderFirstRow.appendAll(pnlHeaderTitle, pnlResources);		
			
			pnlHeader.removeAll();
			pnlHeader.append(pnlHeaderFirstRow);			
			
			// Needed since gets called after the panel has already been rendered (for updates)
			pnlHeader.repaintAndRevalidate();
			pnlInfoContainer.repaintAndRevalidate();
			
			return pnlInfoContainer;
		}
		
		public function ReceiveNewMessage(): void{
			messageBoard.showRefreshButton();
		}
	}
	
}