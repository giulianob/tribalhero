package src.UI.Dialog{

    import flash.events.Event;

    import org.aswing.*;
    import org.aswing.event.AWEvent;
    import org.aswing.event.TableCellEditEvent;
    import org.aswing.geom.*;
    import org.aswing.table.GeneralTableCellFactory;
    import org.aswing.table.PropertyTableModel;

    import src.Comm.GameURLLoader;
    import src.Global;
    import src.UI.Components.Messaging.PreviewTextCell;
    import src.UI.Components.PagingBar;
    import src.UI.Components.TableCells.CheckboxTextCell;
    import src.UI.GameJPanel;
    import src.Util.DateUtil;

    public class MessagingDialog extends GameJPanel {

		private var loader: GameURLLoader;
		private var actionLoader: GameURLLoader;

		private var messageList: VectorListModel;
		private var messageModel: PropertyTableModel;
		private var messageTable: JTable;

		private var btnInboxMarkAsRead: JButton;
		private var btnInboxDelete: JButton;

		private var btnSentDelete: JButton;

		private var pagingBar: PagingBar;
		
		private var tabs: JTabbedPane;

		private var pnlInbox: JPanel;
		private var pnlSent: JPanel;
		
		private var btnNewMessage: JButton;

		private var refreshOnClose: Boolean = false;
		
		private var scrollInbox: JScrollPane;
		private var scrollSent: JScrollPane;	
		
		private var dummyInboxViewport: JPanel;
		private var dummySentViewport: JPanel;

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
			if (tabs.getSelectedIndex() == 1 && pagingBar.page < 2) {
				pagingBar.refreshPage(1);
			}
		}

		private function deleteChecked(e: Event = null) : void {
			var ids: Array = getCheckedMessagesIds();

			if (ids.length == 0) {
				InfoDialog.showMessageDialog("Message", "No messages are selected.");
				return;
			}

			Global.mapComm.Messaging.del(actionLoader, ids);
			
			refreshOnClose = true;
		}

		private function deleteMessage(id: int) : void {
			var ids: Array = [];
			ids.push(id);
			Global.mapComm.Messaging.del(actionLoader, ids);
		}

		private function markAsRead(e: Event = null) : void {
			var ids: Array = getCheckedMessagesIds();

			if (ids.length == 0) {
				InfoDialog.showMessageDialog("Message", "No messages are selected.");
				return;
			}

			Global.mapComm.Messaging.markAsRead(actionLoader, ids);
			
			refreshOnClose = true;
		}

		// Use the cell editing ability to decide if the user clicked the preview box which then we show the message
		private function onEditing(e: TableCellEditEvent) : void {
			var selectedColumn: int = e.getColumn();
			var selectedRow: int = e.getRow();

			messageTable.getCellEditor().cancelCellEditing();

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
						pagingBar.refreshPage();
					}
				});
			});

			Global.mapComm.Messaging.view(messageLoader, id);
		}

		private function getCheckedMessagesIds() : Array {
			var ids: Array = [];

			for (var i: int = 0; i < messageTable.getRowCount(); i++) {
				if (messageTable.getValueAt(i, 0).checked) {
					ids.push(messageList.get(i).id);
				}
			}

			return ids;
		}

		private function onTabChanged(e: AWEvent) : void {
			if (tabs.getSelectedIndex() == 0) {
				scrollSent.setView(dummySentViewport);
				scrollInbox.setView(messageTable);
			} else {
				scrollInbox.setView(dummyInboxViewport);
				scrollSent.setView(messageTable);
			}
			
			pagingBar.refreshPage(1);
		}

		private function loadPage(page: int) : void {
			if (tabs.getSelectedIndex() == 0)
				Global.mapComm.Messaging.list(loader, "inbox", page);
			else
				Global.mapComm.Messaging.list(loader, "sent", page);			
		}

		private function onLoadMessages(e: Event) : void {
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

			pagingBar.setData(data);

			messageList.clear();

			for each(var message: Object in data.messages) {
				messageList.append(message);
			}
		}

		private function onActionReply(e: Event) : void {
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

			pagingBar.refreshPage();
		}

		public function show(owner:* = null, modal:Boolean = true, onClose: Function = null) :JFrame
		{
			super.showSelf(owner, modal, onClose);
			Global.gameContainer.showFrame(frame);

			pagingBar.refreshPage();

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

        private function dateTranslator(message: *, key: String) : String {
            return DateUtil.niceShort(message.date);
        }

		private function createUI():void {
			title = "Messages";
			setLayout(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 5));
					
			// Messages Table
			messageList = new VectorListModel();

			messageModel = new PropertyTableModel(messageList,
			["", "Player", "Subject", "Date"],
			[".", "name", ".", "date"],
			[null, nameTranslator, null, dateTranslator]
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
			messageTable.getColumnAt(2).setPreferredWidth(435);
			messageTable.getColumnAt(3).setPreferredWidth(125);

			// Inbox buttons
			btnInboxMarkAsRead = new JButton("Mark as Read");
			btnInboxDelete = new JButton("Delete");
					
			// Scroll panes
			dummyInboxViewport = new JPanel();
			dummySentViewport = new JPanel();
			
			scrollInbox = new JScrollPane(messageTable, JScrollPane.SCROLLBAR_AS_NEEDED, JScrollPane.SCROLLBAR_NEVER);
			scrollSent = new JScrollPane(dummySentViewport, JScrollPane.SCROLLBAR_AS_NEEDED, JScrollPane.SCROLLBAR_NEVER);
			
			pnlInbox = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 5));
			var pnlInboxButtons: JPanel = new JPanel();
			pnlInboxButtons.append(btnInboxMarkAsRead);
			pnlInboxButtons.append(btnInboxDelete);
			pnlInbox.append(pnlInboxButtons);
			pnlInbox.append(scrollInbox);

			// Sent buttons
			btnSentDelete = new JButton("Delete");

			pnlSent = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 5));
			var pnlSentButtons: JPanel = new JPanel();
			pnlSentButtons.append(btnSentDelete);
			pnlSent.append(pnlSentButtons);
			pnlSent.append(scrollSent);

			// Bottom bar
			var pnlFooter: JPanel = new JPanel(new BorderLayout(10));

			// Paging
			pagingBar = new PagingBar(loadPage);
			pagingBar.setConstraints("West");

			// New Message footer
			var pnlNewMessage: JPanel = new JPanel();
			pnlNewMessage.setConstraints("East");

			btnNewMessage = new JButton("New Message");

			// Tabs
			tabs = new JTabbedPane();
			tabs.appendTab(pnlInbox, "Inbox");
			tabs.appendTab(pnlSent, "Sent");

			//component layoution
			pnlNewMessage.append(btnNewMessage);

			pnlFooter.append(pagingBar);
			pnlFooter.append(pnlNewMessage);

			append(tabs);
			append(pnlFooter);
		}
	}
}

