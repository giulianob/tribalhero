package src.UI.Dialog 
{
	import adobe.utils.CustomActions;
	import src.Util.StringHelper;
	import flash.events.*;
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
	import src.UI.*;
	import src.UI.Components.*;
	import src.UI.Components.TableCells.*;
	import src.UI.Components.Tribe.*;
	import src.UI.Cursors.GroundAttackCursor;
	import src.UI.LookAndFeel.*;
	import src.UI.Tooltips.*;
	import src.Map.Username;
	import src.Util.Util;
	
	public class TribePublicProfileDialog extends GameJPanel
	{
		private var profileData: * ;
		
		private var pnlHeader: JPanel;
		private var pnlInfoContainer: JPanel;
		private var lblTribeName: JLabel;
		
		private var pnlTabs: JTabbedPane;
		private var pnlInfoTabs: JTabbedPane;
		
		public function TribePublicProfileDialog(profileData: *) 
		{
			this.profileData = profileData;
			
			createUI();
		}

		public function show(owner:* = null, modal:Boolean = true, onClose: Function = null):JFrame 
		{
			super.showSelf(owner, modal, onClose);
			Global.gameContainer.closeAllFramesByType(TribePublicProfileDialog);
			Global.gameContainer.showFrame(frame);
			return frame;
		}
		
		private function addInfo(form: Form, title: String, text: *) : void {
			var rowTitle: JLabel = new JLabel(title, null, AsWingConstants.LEFT);
			rowTitle.setName("title");

			var label: JLabel = new JLabel(text, null, AsWingConstants.LEFT);
			label.setName("value");

			form.addRow(rowTitle, label);
		}
						
		private function createUI():void {
			setPreferredSize(new IntDimension(Math.min(375, Constants.screenW - GameJImagePanelBackground.getFrameWidth()) , Math.min(600, Constants.screenH - GameJImagePanelBackground.getFrameHeight())));
			
			title = "Tribe Profile - " + profileData.tribeName;
			setLayout(new BorderLayout(0, 15));
			
			// Header panel
			pnlHeader = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 5));
			pnlHeader.setConstraints("North");		
			lblTribeName = new JLabel(profileData.tribeName, null, AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(lblTribeName, "darkSectionHeader");
			
			var stats: Form = new Form();
			
			var establishedDiff:int = Global.map.getServerTime() - profileData.created;
			addInfo(stats, StringHelper.localize("STR_LEVEL"), profileData.level);
			addInfo(stats, StringHelper.localize("STR_ESTABLISHED"), Util.niceDays(establishedDiff));
			
			pnlHeader.appendAll(lblTribeName, stats);
			
			// Tab panel
			pnlTabs = new JTabbedPane();
			pnlTabs.setPreferredSize(new IntDimension(375, 600));
			pnlTabs.setConstraints("Center");

			// Append tabs			
			pnlTabs.appendTab(createMembersTab(), StringUtil.substitute("Members ({0})", profileData.members.length));
			
			// Append main panels
			appendAll(pnlHeader, pnlTabs);
		}
		
		private function createMembersTab() : Container {
			var modelMembers: VectorListModel = new VectorListModel(profileData.members);
			var tableMembers: JTable = new JTable(new PropertyTableModel(
				modelMembers, 
				["Player", "Rank"],
				[".", "rank"],
				[null, new TribeRankTranslator()]
			));			
			tableMembers.addEventListener(TableCellEditEvent.EDITING_STARTED, function(e: TableCellEditEvent) : void {
				tableMembers.getCellEditor().cancelCellEditing();
			});			
			tableMembers.setRowSelectionAllowed(false);
			tableMembers.setAutoResizeMode(JTable.AUTO_RESIZE_OFF);
			tableMembers.getColumnAt(0).setPreferredWidth(145);
			tableMembers.getColumnAt(0).setCellFactory(new GeneralTableCellFactory(PlayerLabelCell));
			tableMembers.getColumnAt(1).setPreferredWidth(145);
			
			var scrollMembers: JScrollPane = new JScrollPane(tableMembers, JScrollPane.SCROLLBAR_ALWAYS, JScrollPane.SCROLLBAR_NEVER);
			
			return scrollMembers;
		}
	}
	
}