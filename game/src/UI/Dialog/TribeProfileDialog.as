package src.UI.Dialog 
{
	import flash.events.*;
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
		
		private var btnUpgrade: JLabelButton;
		private var btnDonate: JLabelButton;
		private var btnInvite: JLabelButton;
		private var btnSetDescription: JLabelButton;
		private var btnDismantle: JLabelButton;
		private var btnLeave: JLabelButton;
		
		public function TribeProfileDialog(profileData: *) 
		{
			this.profileData = profileData;
			
			createUI();
			
			btnSetDescription.addActionListener(function(e: Event = null): void {
				
				var pnl: JPanel = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 5));
				var txtDescription: JTextArea = new JTextArea(profileData.description, 10, 10);
				txtDescription.setMaxChars(1000);
				
				var scrollDescription: JScrollPane = new JScrollPane(txtDescription, JScrollPane.SCROLLBAR_AS_NEEDED, JScrollPane.SCROLLBAR_AS_NEEDED);			
			
				pnl.appendAll(new JLabel("Set a message to appears on the tribe profile. This will only be visible to your tribe members.", null, AsWingConstants.LEFT), scrollDescription);
				InfoDialog.showMessageDialog("Say something to your tribe", pnl, function(msg: String = ""): void {
					Global.mapComm.Tribe.setTribeDescription(txtDescription.getText());
					update();
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
		}
		
		public function update(): void {
			getFrame().dispose();
			
			Global.mapComm.Tribe.viewTribeProfile(function(newProfileData: *): void {
				if (!newProfileData) 
					return;
				
				var dialog: TribeProfileDialog = new TribeProfileDialog(newProfileData);
				dialog.show();
			});			
		}
		
		public function show(owner:* = null, modal:Boolean = true, onClose: Function = null):JFrame 
		{
			super.showSelf(owner, modal, onClose);
			Global.gameContainer.showFrame(frame);
			return frame;
		}
		
		private function createUI():void {
			setPreferredSize(new IntDimension(775, 375));
			title = "Tribe Profile - " + profileData.tribeName;
			setLayout(new BorderLayout(5));
			
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
			
			var pnlActions: JPanel = new JPanel(new FlowLayout(AsWingConstants.LEFT, 10, 0, false));
			btnDonate = new JLabelButton("Donate");
			btnSetDescription = new JLabelButton("Set Announcement");
			btnInvite = new JLabelButton("Invite");
			btnUpgrade = new JLabelButton("Upgrade");
			btnDismantle = new JLabelButton("Dismantle");
			btnLeave = new JLabelButton("Leave");
			
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
			
			pnlHeader.append(pnlHeaderFirstRow);
			
			pnlHeader.appendAll(new JLabel(" "), pnlActions);
			
			// description
			var description: String = profileData.description == "" ? "The tribe chief hasn't set an announcement yet" : profileData.description;
			var lblDescription: MultilineLabel = new MultilineLabel(description);
			lblDescription.setPreferredWidth(325);
			lblDescription.setBackgroundDecorator(new GamePanelBackgroundDecorator("TabbedPane.top.contentRoundImage"));
			lblDescription.setBorder(new EmptyBorder(null, UIManager.get("TabbedPane.contentMargin") as Insets));
			
			var scrollDescription: JScrollPane = new JScrollPane(new JViewport(lblDescription));
			(scrollDescription.getViewport() as JViewport).setVerticalAlignment(AsWingConstants.TOP);
			(scrollDescription.getViewport() as JViewport).setHorizontalAlignment(AsWingConstants.LEFT);
			scrollDescription.setConstraints("Center");
			
			// Create west panel
			var pnlWest: JPanel = new JPanel(new BorderLayout());
			pnlWest.setConstraints("Center");
			pnlWest.append(pnlHeader);
			pnlWest.append(scrollDescription);
			
			// Tab panel
			var pnlTabs: JTabbedPane = new JTabbedPane();
			pnlTabs.setPreferredSize(new IntDimension(375, 350));
			pnlTabs.setConstraints("East");
			
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

			var scrollMembers: JScrollPane = new JScrollPane(new JViewport(tableMembers, true, false), JScrollPane.SCROLLBAR_AS_NEEDED, JScrollPane.SCROLLBAR_NEVER);
			(scrollMembers.getViewport() as JViewport).setVerticalAlignment(AsWingConstants.TOP);
			
			// Append tabs			
			pnlTabs.appendTab(scrollMembers, "Members (" + profileData.members.length + ")");			
			
			// Append main panels
			append(pnlWest);
			append(pnlTabs);
		}
	}
	
}