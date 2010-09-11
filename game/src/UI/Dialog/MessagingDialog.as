package src.UI.Dialog{

	import flash.events.Event;
	import org.aswing.event.AWEvent;
	import org.aswing.event.TableCellEditEvent;
	import org.aswing.table.GeneralTableCellFactory;
	import org.aswing.table.PropertyTableModel;
	import src.Comm.GameURLLoader;
	import src.Global;
	import src.UI.Components.Messaging.PreviewTextCell;
	import src.UI.Components.TableCells.CheckboxTextCell;
	import src.UI.GameJPanel;
	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.geom.*;
	import org.aswing.colorchooser.*;
	import org.aswing.ext.*;

	public class MessagingDialog extends GameJPanel {

		private var loader: GameURLLoader;
		private var actionLoader: GameURLLoader;
		private var page: int = 1;
		private var pnlPaging:JPanel;
		private var btnPrevious:JLabelButton;
		private var btnFirst:JLabelButton;
		private var lblPages:JLabel;
		private var btnNext:JLabelButton;

		private var pnlLoading: GameJPanel;

		private var messageList: VectorListModel;
		private var messageModel: PropertyTableModel;
		private var messageTable: JTable;

		private var btnInboxMarkAsRead: JButton;
		private var btnInboxDelete: JButton;

		private var btnSentDelete: JButton;

		private var tabs: JTabbedPane;

		private var pnlInbox: JPanel;
		private var pnlTrash: JPanel;
		private var pnlSent: JPanel;

		private var btnNewMessage: JButton;
		
		private var refreshOnClose: Boolean = false;

		public function MessagingDialog() {
			loader = new GameURLLoader();
			loader.addEventListener(Event.COMPLETE, onLoadMessages);

			actionLoader = new GameURLLoader();
			actionLoader.addEventListener(Event.COMPLETE, onActionReply);

			createUI();

			// Disables editing the table
			messageTable.addEventListener(TableCellEditEvent.EDITING_STARTED, onEditing);

			// General Buttons
			btnNewMessage.addActionListener(onNewMessage);

			// Inbox buttons
			btnInboxDelete.addActionListener(deleteChecked);
			btnInboxMarkAsRead.addActionListener(markAsRead);

			// Sent buttons
			btnSentDelete.addActionListener(deleteChecked);

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

			tabs.addStateListener(onTabChanged);
		}

		public function getRefreshOnClose() : Boolean {
			return refreshOnClose;
		}
		
		private function onNewMessage(e: Event = null) : void {
			var newMessageDialog: MessageCreateDialog = new MessageCreateDialog(onNewMessageSend);
			newMessageDialog.show();
		}

		private function onNewMessageSend(dialog: MessageCreateDialog) : void {
			dialog.getFrame().dispose();
			// Refresh if on send tab
			if (tabs.getSelectedIndex() == 1 && page < 2) {
				loadPage(1);
			}
		}

		private function deleteChecked(e: Event = null) : void {
			var ids: Array = getCheckedMessagesIds();

			if (ids.length == 0) {
				InfoDialog.showMessageDialog("Message", "No messages are selected.");
				return;
			}

			pnlLoading = InfoDialog.showMessageDialog("Deleting", "Deleting messages...", null, null, true, false, 0);
			Global.mapComm.Messaging.del(actionLoader, ids);
		}

		private function deleteMessage(id: int) : void {
			pnlLoading = InfoDialog.showMessageDialog("Deleting", "Deleting message...", null, null, true, false, 0);
			var ids: Array = new Array();
			ids.push(id);
			Global.mapComm.Messaging.del(actionLoader, ids);
		}

		private function markAsRead(e: Event = null) : void {
			var ids: Array = getCheckedMessagesIds();

			if (ids.length == 0) {
				InfoDialog.showMessageDialog("Message", "No messages are selected.");
				return;
			}

			pnlLoading = InfoDialog.showMessageDialog("Marking as Read", "Marking messages...", null, null, true, false, 0);
			Global.mapComm.Messaging.markAsRead(actionLoader, ids);
		}

		// Use the cell editing ability to decide if the user clicked the preview box which then we show the message
		private function onEditing(e: TableCellEditEvent) : void {
			var selectedColumn: int = e.getColumn();
			var selectedRow: int = e.getRow();

			messageTable.getCellEditor().stopCellEditing();

			if (selectedColumn != 2 || selectedRow == -1) {
				return;
			}

			var message: * = messageList.get(selectedRow);
			viewMessage(message.id);
		}

		private function viewMessage(id: int) : void {
			var messageLoader: GameURLLoader = new GameURLLoader();
			messageLoader.addEventListener(Event.COMPLETE, function(event: Event) : void {
				var data: Object;
				try
				{
					data = messageLoader.getDataAsObject();
				}
				catch (e: Error) {
					InfoDialog.showMessageDialog("Error", "Unable to query message. Try again later.");
					return;
				}

				if (data.error != null && data.error != "") {
					InfoDialog.showMessageDialog("Info", data.error);
					return;
				}

				var viewMessageDialog: MessageViewDialog = new MessageViewDialog(data.message, function(viewDialog: MessageViewDialog) : void {
					viewDialog.getFrame().dispose();
					deleteMessage(data.message.id);
				});

				viewMessageDialog.show(null, true, function(viewDialog: MessageViewDialog = null) : void {
					if (data.refreshOnClose) {
						refreshOnClose = true;
						loadPage(page);
					}
				});
			});

			Global.mapComm.Messaging.view(messageLoader, id);
		}

		private function getCheckedMessagesIds() : Array {
			var ids: Array = new Array();

			for (var i: int = 0; i < messageTable.getRowCount(); i++) {
				if (messageTable.getValueAt(i, 0).checked) {
					ids.push(messageList.get(i).id);
				}
			}

			return ids;
		}

		private function onTabChanged(e: AWEvent) : void {
			messageTable.getParent().remove(messageTable);
			(tabs.getSelectedComponent() as Container).append(messageTable);
			(tabs.getSelectedComponent() as Container).pack();
			loadPage(1);
		}

		private function loadPage(page: int) : void {
			pnlLoading = InfoDialog.showMessageDialog("Loading", "Loading messages...", null, null, true, false, 0);

			if (tabs.getSelectedIndex() == 0) {
				Global.mapComm.Messaging.list(loader, "inbox", page);
			} else {
				Global.mapComm.Messaging.list(loader, "sent", page);
			}
		}

		private function onLoadMessages(e: Event) : void {
			pnlLoading.getFrame().dispose();

			var data: Object;
			try
			{
				data = loader.getDataAsObject();
			}
			catch (e: Error) {
				InfoDialog.showMessageDialog("Error", "Unable to query messages. Try again later.");
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

			messageList.clear();

			for each(var message: Object in data.messages) {
				messageList.append(message);
			}
		}

		private function onActionReply(e: Event) : void {
			pnlLoading.getFrame().dispose();

			var data: Object;
			try
			{
				data = actionLoader.getDataAsObject();
			}
			catch (e: Error) {
				InfoDialog.showMessageDialog("Error", "Unable to perform this action. Try again later.");
				return;
			}

			if (data.error != null) {
				InfoDialog.showMessageDialog("Info", data.error);
				return;
			}

			loadPage(page);
		}

		public function show(owner:* = null, modal:Boolean = true, onClose: Function = null) :JFrame
		{
			super.showSelf(owner, modal, onClose);
			Global.gameContainer.showFrame(frame);

			loadPage(page);

			return frame;
		}

		private function nameTranslator(message: *, key: String) : String {
			if (message.isRecipient || message.name.substr(0, 4) == "To: ") {
				return message.name;
			}
			else {
				return "To: " + message.name;
			}
		}

		private function createUI():void {
			title = "Messages";
			setLayout(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 5));

			// Messages Table
			messageList = new VectorListModel();

			messageModel = new PropertyTableModel(messageList,
			["", "Player", "Subject", "Date"],
			[".", "name", ".", "date"],
			[null, nameTranslator, null, null]
			);

			messageTable = new JTable(messageModel);
			(messageTable.getDefaultEditor("Object") as AbstractCellEditor).setClickCountToStart(1);
			messageTable.setRowSelectionAllowed(false);
			messageTable.setAutoResizeMode(JTable.AUTO_RESIZE_OFF);
			messageTable.setDefaultEditor("", null);
			messageTable.setPreferredSize(new IntDimension(700, 300));

			messageTable.getColumnAt(0).setCellFactory(new GeneralTableCellFactory(CheckboxTextCell));
			messageTable.getColumnAt(2).setCellFactory(new GeneralTableCellFactory(PreviewTextCell));

			messageTable.setRowHeight(23);

			messageTable.getColumnAt(0).setPreferredWidth(25);
			messageTable.getColumnAt(1).setPreferredWidth(110);
			messageTable.getColumnAt(2).setPreferredWidth(460);
			messageTable.getColumnAt(3).setPreferredWidth(100);

			// Inbox buttons
			btnInboxMarkAsRead = new JButton("Mark as Read");
			btnInboxDelete = new JButton("Delete");

			pnlInbox = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 5));
			var pnlInboxButtons: JPanel = new JPanel();
			pnlInboxButtons.append(btnInboxMarkAsRead);
			pnlInboxButtons.append(btnInboxDelete);
			pnlInbox.append(pnlInboxButtons);
			pnlInbox.append(messageTable);

			// Sent buttons
			btnSentDelete = new JButton("Delete");

			pnlSent = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 5));
			var pnlSentButtons: JPanel = new JPanel();
			pnlSentButtons.append(btnSentDelete);
			pnlSent.append(pnlSentButtons);

			// Bottom bar
			var pnlFooter: JPanel = new JPanel(new BorderLayout(10));

			// Paging
			pnlPaging = new JPanel();
			pnlPaging.setConstraints("West");

			btnFirst = new JLabelButton("<< Newest");
			btnPrevious = new JLabelButton("< Newer");
			btnNext = new JLabelButton("Older >");

			lblPages = new JLabel();

			// New Message footer
			var pnlNewMessage: JPanel = new JPanel();
			pnlNewMessage.setConstraints("East");

			btnNewMessage = new JButton("New Message");

			// Tabs
			tabs = new JTabbedPane();
			
			var scrollInbox: JScrollPane = new JScrollPane(pnlInbox);
			tabs.appendTab(scrollInbox, "Inbox");
			
			var scrollSent: JScrollPane = new JScrollPane(pnlSent);
			tabs.appendTab(scrollSent, "Sent");

			//component layoution
			pnlPaging.append(btnFirst);
			pnlPaging.append(btnPrevious);
			pnlPaging.append(lblPages);
			pnlPaging.append(btnNext);

			pnlNewMessage.append(btnNewMessage);

			pnlFooter.append(pnlPaging);
			pnlFooter.append(pnlNewMessage);

			append(tabs);
			append(pnlFooter);
		}
	}
}

