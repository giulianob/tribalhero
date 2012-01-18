package src.UI.Dialog 
{
	import adobe.utils.CustomActions;
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
	import src.Objects.Process.AssignmentCreateProcess;
	import src.Objects.Process.AssignmentJoinProcess;
	import src.UI.*;
	import src.UI.Components.*;
	import src.UI.Components.TableCells.*;
	import src.UI.Components.Tribe.*;
	import src.UI.Cursors.GroundAttackCursor;
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
		
		public function TribeProfileDialog(profileData: *) 
		{
			this.profileData = profileData;
			
			createUI();
			
			messageBoard.loadThreadPage();
			
			updateTimer = new Timer(60 * 5 * 1000);
			updateTimer.addEventListener(TimerEvent.TIMER, function(e: Event = null): void {
				update();
			});			
			updateTimer.start();
		}
		
		private function dispose():void {
			updateTimer.stop();
		}
		
		public function update(): void {
			Global.mapComm.Tribe.viewTribeProfile(function(newProfileData: *): void {
				if (!newProfileData) 
					return;
				
				profileData = newProfileData;
				createInfoTab();				
			});
		}
		
		public function show(owner:* = null, modal:Boolean = true, onClose: Function = null):JFrame 
		{
			super.showSelf(owner, modal, onClose, dispose);
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
			pnlTabs.appendTab(createMessageBoardTab(), "Message Board");
			
			// Append main panels
			appendAll(pnlHeader, pnlTabs);
		}
	
		private function createMessageBoardTab(): Container {		
			messageBoard = new MessageBoard();					
			return messageBoard;
		}
		
		private function createIncomingPanelItem(incoming: *): JPanel {
			var pnlContainer: JPanel = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 0));
			
			var pnlHeader: JPanel = new JPanel(new FlowLayout(AsWingConstants.LEFT, 0, 0, false));
			
			pnlHeader.appendAll(
				new PlayerCityLabel(incoming.targetPlayerId, incoming.targetCityId, incoming.targetPlayerName, incoming.targetCityName),
				new JLabel(" is being attacked by ", null, AsWingConstants.LEFT),
				new PlayerCityLabel(incoming.sourcePlayerId, incoming.sourceCityId, incoming.sourcePlayerName, incoming.sourceCityName)
			);
			
			var lblCountdown: CountDownLabel = new CountDownLabel(incoming.endTime, "Battle In Progress");
			
			pnlContainer.appendAll(pnlHeader, lblCountdown);
			
			return pnlContainer;
		}
		
		private function createAssignmentItem(assignment: *): JPanel {
			var pnlContainer: JPanel = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 0));
			
			var pnlName: JPanel = new JPanel(new FlowLayout(AsWingConstants.LEFT, 0, 0, false));			
			pnlName.appendAll(				
				new JLabel("Attack", null, AsWingConstants.LEFT),
				new PlayerCityLabel(assignment.targetPlayerId, assignment.targetCityId, assignment.targetPlayerName, assignment.targetCityName)
			);						
			pnlName.setConstraints("Center");
			
			var pnlStats: JPanel = new JPanel(new SoftBoxLayout(AsWingConstants.RIGHT, 5, AsWingConstants.RIGHT));
			pnlStats.appendAll(
				new JLabel(assignment.troopCount, new AssetIcon(new ICON_SINGLE_SWORD()), AsWingConstants.RIGHT),
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
				var join: AssignmentJoinProcess = new AssignmentJoinProcess(assignment);
				join.execute();
			});			
						
			return pnlContainer;
		}		
		
		private function createAssignmentTab() : Container {
			var btnCreate: JButton = new JButton("Create");
			
			var pnlFooter: JPanel = new JPanel(new FlowLayout(AsWingConstants.RIGHT));
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
			
			btnCreate.addActionListener(function(e: Event): void {
				var assignmentCreate: AssignmentCreateProcess = new AssignmentCreateProcess();
				assignmentCreate.execute();
			});
			new SimpleTooltip(btnCreate, "An assignment is an organized attack used by the tribe to dispatch troops automatically at different times, so all of them can start the battle at the same time regardless of the distance/speed.");
			
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
				tableMembers.getCellEditor().stopCellEditing();
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
			
			var pnlActions: JPanel = new JPanel(new FlowLayout(AsWingConstants.LEFT, 10, 0, false));				
			
			// Show correct buttons depending on rank
			switch (Constants.tribeRank) {
				case 0: 
					pnlActions.appendAll(btnSetDescription, btnInvite, btnDonate, btnUpgrade, btnDismantle);
					break;
				case 1:
					pnlActions.appendAll(btnInvite, btnDonate, btnLeave);
					break;
				default:
					pnlActions.appendAll(btnDonate, btnLeave);
					break;
			}
			
			// upgrade btn
			var upgradeTooltip: TribeUpgradeTooltip = new TribeUpgradeTooltip(profileData.tribeLevel, profileData.resources);
			upgradeTooltip.bind(btnUpgrade);
			btnUpgrade.addActionListener(function(e: Event): void {
				Global.mapComm.Tribe.upgrade();
			});
			
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
				
				var invitePlayerName: InfoDialog = InfoDialog.showMessageDialog("Invite a new tribesman", pnl, function(response: *) : void {
					if (txtPlayerName.getLength() > 0)
						Global.mapComm.Tribe.invitePlayer(txtPlayerName.getText());
				});				
			});
			
			btnDismantle.addActionListener(function(e: Event): void {
				var invitePlayerName: InfoDialog = InfoDialog.showInputDialog("Dismantle tribe", "If you really want to dismantle your tribe then type 'delete' below and click ok.", function(input: *) : void {
					if (input == "'delete'" || input == "delete")
						Global.mapComm.Tribe.dismantle();
				});				
			});			
			
			btnLeave.addActionListener(function(e: Event): void {
				var invitePlayerName: InfoDialog = InfoDialog.showMessageDialog("Leave tribe", "Do you really want to leave the tribe?", function(result: *) : void {
					if (result == JOptionPane.YES)
						Global.mapComm.Tribe.leave();
				}, null, true, true, JOptionPane.YES | JOptionPane.NO);				
			});		
			
			btnDonate.addActionListener(function(e: Event): void {
				var tribeContributeDialog: TribeContributeDialog = new TribeContributeDialog(function(dialog: TribeContributeDialog): void {
					dialog.getFrame().dispose();
					update();
				});
				tribeContributeDialog.show();
			});
			
			// First row of header panel which contains player name + ranking
			var pnlHeaderFirstRow: JPanel = new JPanel(new BorderLayout(5));
			
			var lblTribeName: JLabel = new JLabel(profileData.tribeName + " (Level " + profileData.tribeLevel + ")", null, AsWingConstants.LEFT);	
			lblTribeName.setConstraints("Center");
			GameLookAndFeel.changeClass(lblTribeName, "darkHeader");			
			
			var pnlResources: JPanel = new JPanel(new FlowLayout(AsWingConstants.RIGHT, 10, 0, false));
			pnlResources.setConstraints("East");
			
			pnlResources.append(new SimpleResourcesPanel(profileData.resources, false));
			
			pnlHeaderFirstRow.appendAll(lblTribeName, pnlResources);		
			
			pnlHeader.removeAll();
			pnlHeader.append(pnlHeaderFirstRow);			
			
			// Needed since gets called after the panel has already been rendered (for updates
			pnlHeader.repaintAndRevalidate();
			pnlInfoContainer.repaintAndRevalidate();
			
			return pnlInfoContainer;
		}
		
		public function ReceiveNewMessage(): void{
			messageBoard.showRefreshButton();
		}
	}
	
}