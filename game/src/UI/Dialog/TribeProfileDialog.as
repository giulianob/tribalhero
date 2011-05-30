package src.UI.Dialog 
{
	import adobe.utils.CustomActions;
	import flash.events.*;
	import flash.utils.Timer;
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
	import src.UI.LookAndFeel.*;
	
	public class TribeProfileDialog extends GameJPanel
	{
		private var profileData: * ;
		
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
				pnlInfoContainer.repaintAndRevalidate();				
			});
		}
		
		public function show(owner:* = null, modal:Boolean = true, onClose: Function = null):JFrame 
		{
			super.showSelf(owner, modal, onClose, dispose);
			Global.gameContainer.showFrame(frame);
			return frame;
		}
		
		private function createUI():void {
			setPreferredSize(new IntDimension(Math.min(1025, Constants.screenW - GameJImagePanelBackground.getFrameWidth()) , Math.max(375, Constants.screenH - GameJImagePanelBackground.getFrameHeight() * 2)));
			title = "Tribe Profile - " + profileData.tribeName;
			setLayout(new BorderLayout(10, 10));
			
			// Header panel
			var pnlHeader: JPanel = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS));
			pnlHeader.setConstraints("North");
			
			// First row of header panel which contains player name + ranking
			var pnlHeaderFirstRow: JPanel = new JPanel(new BorderLayout(5));
			
			var lblTribeName: JLabel = new JLabel(profileData.tribeName + " (Level " + profileData.tribeLevel + ")", null, AsWingConstants.LEFT);	
			lblTribeName.setConstraints("Center");
			GameLookAndFeel.changeClass(lblTribeName, "darkHeader");			
			
			var pnlResources: JPanel = new JPanel(new FlowLayout(AsWingConstants.RIGHT, 10, 0, false));
			pnlResources.setConstraints("East");
			
			pnlResources.append(new SimpleResourcesPanel(profileData.resources, false));
			
			pnlHeaderFirstRow.appendAll(lblTribeName, pnlResources);		
			
			pnlHeader.append(pnlHeaderFirstRow);
			
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
			
			// description
			var description: String = profileData.description == "" ? "The tribe chief hasn't set an announcement yet" : profileData.description;
			var lblDescription: MultilineLabel = new MultilineLabel(description);
			
			var scrollDescription: JScrollPane = new JScrollPane(lblDescription);
			
			// Side tab browser
			var lastActiveInfoTab: int = 0;
			if (pnlInfoTabs)
				lastActiveInfoTab = pnlInfoTabs.getSelectedIndex();
				
			pnlInfoTabs = new JTabbedPane();
			
			// Members tab
			var modelMembers: VectorListModel = new VectorListModel(profileData.members);
			var tableMembers: JTable = new JTable(new PropertyTableModel(
				modelMembers, 
				["Player", "Rank", ""],
				[".", "rank", "."],
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
			tableMembers.getColumnAt(2).setCellFactory(new GeneralTableCellFactory(TribeMemberActionCell));
			tableMembers.getColumnAt(2).setPreferredWidth(70);
			
			var scrollMembers: JScrollPane = new JScrollPane(tableMembers, JScrollPane.SCROLLBAR_ALWAYS, JScrollPane.SCROLLBAR_NEVER);
			
			// Incoming attacks tab
			var pnlIncomingAttacks: JPanel = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 10));
			
			for each (var incoming: * in profileData.incomingAttacks) {
				pnlIncomingAttacks.append(createIncomingPanelItem(incoming));
			}
			
			var scrollIncomingAttacks: JScrollPane = new JScrollPane(new JViewport(pnlIncomingAttacks, true), JScrollPane.SCROLLBAR_ALWAYS, JScrollPane.SCROLLBAR_NEVER);
			(scrollIncomingAttacks.getViewport() as JViewport).setVerticalAlignment(AsWingConstants.TOP);
			
			// Put it all together
			var pnlEast: JPanel = new JPanel(new BorderLayout(0, 5));
			pnlInfoTabs.setConstraints("Center");
			pnlEast.appendAll(pnlInfoTabs);
			
			pnlInfoTabs.appendTab(scrollMembers, "Members (" + profileData.members.length + ")");
			pnlInfoTabs.appendTab(scrollIncomingAttacks, "Incoming Attacks (" + profileData.incomingAttacks.length + ")");

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
				var txtDescription: JTextArea = new JTextArea(profileData.description, 10, 10);
				txtDescription.setMaxChars(3000);
				
				var scrollDescription: JScrollPane = new JScrollPane(txtDescription, JScrollPane.SCROLLBAR_AS_NEEDED, JScrollPane.SCROLLBAR_AS_NEEDED);			
			
				pnl.appendAll(new JLabel("Set a message to appears on the tribe profile. This will only be visible to your tribe members.", null, AsWingConstants.LEFT), scrollDescription);
				InfoDialog.showMessageDialog("Say something to your tribe", pnl, function(result: * ): void {
					if (result == JOptionPane.CANCEL || result == JOptionPane.CLOSE)
						return;
						
					Global.mapComm.Tribe.setTribeDescription(txtDescription.getText());					
				});
			});
			
			btnInvite.addActionListener(function(e: Event): void {
				var invitePlayerName: InfoDialog = InfoDialog.showInputDialog("Invite a new tribesman", "Type in the name of the player you want to invite", function(playerName: *) : void {
					if (playerName != null && playerName != "")
						Global.mapComm.Tribe.invitePlayer(playerName);
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
			
			return pnlInfoContainer;
		}
	}
	
}