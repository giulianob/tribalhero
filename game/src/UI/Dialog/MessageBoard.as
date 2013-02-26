package src.UI.Dialog 
{
	import flash.events.*;
	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.event.*;
	import org.aswing.ext.*;
	import org.aswing.table.*;
	import src.*;
	import src.Comm.*;
	import src.UI.*;
	import src.UI.Components.*;
	import src.UI.Components.TableCells.*;
	import src.UI.LookAndFeel.*;
	import src.Util.StringHelper;

	public class MessageBoard extends GameJPanel
	{
		private var modelThreads: VectorListModel;		
		private var tableThreads: JTable;	
		
		private var pnlPostItemContainer: JPanel;
		private var pnlPostBottomContainer: JPanel;
		private var pnlPostContainer: JPanel;
		private var scrollPosts: JScrollPane;
		private var pnlNewMessage:JPanel;
		
		private var threadLoader: GameURLLoader;
		private var postLoader: GameURLLoader;
		private var actionLoader: GameURLLoader;
		
		private var threadPaging: PagingBar;
		private var postPaging: PagingBar;
		
		private var btnNewThread: JButton;
		private var btnNewPost: JButton;
		
		private var pnlNewPost: JPanel;
		private var txtNewPostMessage: JTextArea;
		private var btnNewPostSubmit: JButton;
		private var btnNewPostCancel: JLabelButton;
		
        private var editPostId: int = -1;
		private var lastThreadId: int = -1;
		private var hasLoaded:Boolean;
		
		public function MessageBoard() 
		{
			createUI();
			
			threadLoader = new GameURLLoader();
			actionLoader = new GameURLLoader();
			postLoader = new GameURLLoader();
			
			threadLoader.addEventListener(Event.COMPLETE, onReceiveThreads);
			postLoader.addEventListener(Event.COMPLETE, onReceivePosts);
			actionLoader.addEventListener(Event.COMPLETE, onReceiveActionComplete);

			tableThreads.addSelectionListener(function(e: SelectionEvent) : void {
				if (e.isProgrammatic()) 
					return;
					
				lastThreadId = getSelectedThreadId();				
				loadPostPage(0);
			});					
			
			btnNewThread.addActionListener(function(e: Event = null) : void {			
				var createDlg: MessageBoardThreadCreateDialog = new MessageBoardThreadCreateDialog(afterCreate);
				createDlg.show();
			});			
			
			btnNewPost.addActionListener(function(e: Event = null) : void {
				pnlNewPost.setVisible(true);
				btnNewPost.setVisible(false);
				txtNewPostMessage.requestFocus();
			});
			
			btnNewPostCancel.addActionListener(function(e: Event = null) : void {
				pnlNewPost.setVisible(false);
				btnNewPost.setVisible(true);
                editPostId = -1;
                txtNewPostMessage.setText("");
			});
			
			btnNewPostSubmit.addActionListener(function(e: Event = null) : void {		
				var loader: GameURLLoader = new GameURLLoader();
				loader.addEventListener(Event.COMPLETE, function(e: Event = null): void {
					var data: Object;
					try
					{
						data = loader.getDataAsObject();
					}
					catch (e: Error) {
						InfoDialog.showMessageDialog("Error", "Unable to post reply. Try again later.");
						return;
					}

					if (data.error != null && data.error != "") {
						InfoDialog.showMessageDialog("Info", data.error);
						return;
					}
					
                    editPostId = -1;
					pnlNewPost.setVisible(false);
					btnNewPost.setVisible(true);
					txtNewPostMessage.setText("");
					
					postPaging.refreshPage(data.pages);
				});
				
				if (txtNewPostMessage.getLength() == 0) {
					InfoDialog.showMessageDialog("Error", "Please write a message.");
					return;
				}				
				
				Global.mapComm.MessageBoard.addPost(loader, lastThreadId, editPostId, txtNewPostMessage.getText());
			});
		}
		
		private function onReceiveActionComplete(e: Event): void {
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
			
			threadPaging.refreshPage();
			postPaging.refreshPage();
		}
		
		private function onReceiveThreads(e: Event): void {
			var data: Object;
			try
			{
				data = threadLoader.getDataAsObject();
			}
			catch (e: Error) {
				InfoDialog.showMessageDialog("Error", "Unable to perform this action. Try again later.");
				return;
			}

			if (data.error != null) {
				InfoDialog.showMessageDialog("Info", data.error);
				return;
			}			

			Global.gameContainer.setUnreadForumIcon(false);
			threadPaging.setData(data);
			
			tableThreads.clearSelection();
			
			modelThreads.clear();			
			modelThreads.appendAll(data.messages);
			
			// try to reselect the last selected thread
			if (lastThreadId > -1) {
				for (var i: int = 0; i < data.messages.length; i++) {
					if (data.messages[i].id != lastThreadId) 
						continue;
					
					tableThreads.setRowSelectionInterval(i, i);	
					break;					
				}
			}
			
		}
		
		private function onReceivePosts(e: Event): void {
			var data: Object;
			try
			{
				data = postLoader.getDataAsObject();
			}
			catch (e: Error) {				
				InfoDialog.showMessageDialog("Error", "Unable to perform this action. Try again later.");
				return;
			}

			if (data.error != null) {				
				InfoDialog.showMessageDialog("Info", data.error);
				return;
			}						
			
			// Hide new post and enable new post button
			if (lastThreadId != data.thread.id) {
				txtNewPostMessage.setText("");
				btnNewPost.setVisible(true);
				pnlNewPost.setVisible(false);
			}
			
			// set paging data
			postPaging.setData(data);
			
			pnlPostBottomContainer.setVisible(true);
			
			var lastRead : Number = data.thread.lastReadTimestamp;
			
			// create main post item
			if (postPaging.page <= 1)	
				pnlPostItemContainer.append(createPostItem(data.thread,lastRead));
						
			// create children posts
			for each (var message: * in data.posts)
				pnlPostItemContainer.append(createPostItem(message,lastRead));			
			
			lastThreadId = data.thread.id;
		}
		
		private function createPostItem(postData: * ,lastRead: Number): JPanel {
			var post: JPanel = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 5));
			
			if (postData.subject) {
				var lblSubject: JLabel = new JLabel(postData.subject, null, AsWingConstants.LEFT);
				GameLookAndFeel.changeClass(lblSubject, "darkHeader");
				post.append(lblSubject);
				post.append(new JSeparator());
			}
			
			var lblPlayer: PlayerLabel = new PlayerLabel(postData.playerId, postData.playerName);
			
			var pnlHeader: JPanel = new JPanel(new BorderLayout(5));
			
			var lblCreated: JLabel = new JLabel(postData.createdInWords, null, AsWingConstants.LEFT);
			
			if (!lastRead || lastRead < postData.createdTimestamp) {
				GameLookAndFeel.changeClass(lblCreated, "Message.unread");
			}
			
			var pnlTools: JPanel = new JPanel(new FlowLayout(AsWingConstants.RIGHT, 5));			
			
			lblCreated.setConstraints("East");
			pnlTools.setConstraints("Center");
			lblPlayer.setConstraints("West");
			pnlHeader.appendAll(lblPlayer, pnlTools, lblCreated);
			
			var message: MultilineLabel = new MultilineLabel();			
			GameLookAndFeel.changeClass(message, "Message");			
			message.setColumns(50);
			message.setHtmlText(StringHelper.linkify(postData.message));
			message.pack();			
			
			var scrollMessage: JScrollPane = new JScrollPane(message);						
			scrollMessage.setPreferredHeight(Math.min(500, message.getHeight() + 20));
			
			var pnlPostTools: JPanel = new JPanel(new FlowLayout(AsWingConstants.RIGHT));
			
			var menuTools: JPopupMenu = new JPopupMenu();			
			
			var btnPostTools: JLabelButton = new JLabelButton("Actions", null, AsWingConstants.RIGHT);								
			btnPostTools.addActionListener(function(e: Event): void {
				if (menuTools.isShowing()) {
					menuTools.setVisible(false);					
				}
				else {
					menuTools.show(btnPostTools, 0, btnPostTools.getHeight());
                    AsWingManager.callLater(function(): void {
                        menuTools.setVisible(false);
                    }, 2250);
				}
			});			
			
			if (Constants.tribeRank <= 1 || Constants.playerId == postData.playerId) {				
				menuTools.addMenuItem(postData.subject ? "Delete Thread" : "Delete Post").addActionListener(function(e: Event): void {
					InfoDialog.showMessageDialog("Delete", "Are you sure?", function(result: *):void {
						if (result != JOptionPane.YES)
							return;
							
						if (postData.subject) {
							Global.mapComm.MessageBoard.delThread(actionLoader, postData.id);
							
							lastThreadId = -1;
							clearPostView();
						} else
							Global.mapComm.MessageBoard.delPost(actionLoader, postData.id);
					}, null, true, true, JOptionPane.YES | JOptionPane.NO);
				});
			}
            
            if (Constants.playerId == postData.playerId) {
				menuTools.addMenuItem("Edit").addActionListener(function(e: Event): void {
                    if (postData.subject) {
                        var editDialog: MessageBoardThreadCreateDialog = new MessageBoardThreadCreateDialog(afterCreate, postData.id, postData.subject, postData.message);
                        editDialog.show();
                    }
                    else {
                        editPostId = postData.id;
                        txtNewPostMessage.setText(postData.message);
                        btnNewPost.doClick();
                    }
				});                
            }
			
			// Only append actions if there's something in the menu
			if (menuTools.numChildren) 
				pnlPostTools.append(btnPostTools);
			
			post.appendAll(pnlHeader, scrollMessage, pnlPostTools);
			
			post.setBackgroundDecorator(new GamePanelBackgroundDecorator("TabbedPane.top.contentRoundImage"));
			post.setBorder(new EmptyBorder(null, UIManager.get("TabbedPane.contentMargin") as Insets));		
			
			post.pack();
			
			return post;
		}
		
		public function loadThreadPage(page: int = 0): void {
			Global.mapComm.MessageBoard.listing(threadLoader, page);
		}
		
		public function loadInitially(): void {
			if (!hasLoaded) {
				hasLoaded = true;
				loadThreadPage();
			}
		}
		
		public function getSelectedThreadId() : int {
			var selectedIdx: int = tableThreads.getSelectedRow();
			if (selectedIdx == -1)
				return -1;
				
			return modelThreads.get(selectedIdx).id;
		}
		
		public function loadPostPage(page: int = 0): void {			
			if (lastThreadId == -1)
				return;
			
			clearPostView();
		
			Global.mapComm.MessageBoard.view(postLoader, page, lastThreadId);
		}
		
		public function clearPostView() : void {
			pnlPostItemContainer.removeAll();
			
			pnlPostBottomContainer.setVisible(false);
		}
		
		private function createUI(): void {
			setLayout(new BorderLayout(5));			
			
			// Left Panel which contains threads
			var pnlThreadContainer: JPanel = new JPanel(new BorderLayout(5, 5));
			
			// Table
			modelThreads = new VectorListModel();			
			tableThreads = new JTable(new PropertyTableModel(
				modelThreads, 
				["Threads"],
				["."],
				[null]
			));			
			tableThreads.setSelectionMode(JTable.SINGLE_SELECTION);
			tableThreads.setRowHeight(38);
			tableThreads.getColumnAt(0).setCellFactory(new GeneralTableCellFactory(MessageBoardThreadListCell));
			tableThreads.getColumnAt(0).setPreferredWidth(300);
			tableThreads.addEventListener(TableCellEditEvent.EDITING_STARTED, function(e: TableCellEditEvent) : void {
				tableThreads.getCellEditor().cancelCellEditing();
			});
			
			// Scroll panel to contain thread table
			var scrollThreads: JScrollPane = new JScrollPane(tableThreads, JScrollPane.SCROLLBAR_ALWAYS, JScrollPane.SCROLLBAR_NEVER);
			scrollThreads.setConstraints("Center");
			
			// Bottom panel in left side which contains paging bar, etc..
			var pnlThreadBottomContainer: JPanel = new JPanel(new BorderLayout(5, 5));
			pnlThreadBottomContainer.setConstraints("South");
			
			// Thread paging 
			threadPaging = new PagingBar(loadThreadPage, false, true, true, true, true, false);
			threadPaging.setConstraints("Center");
			
			// New thread button
			btnNewThread = new JButton("New Thread");
			btnNewThread.setConstraints("East");
			
			pnlThreadBottomContainer.appendAll(threadPaging, btnNewThread);						
			pnlThreadContainer.appendAll(scrollThreads, pnlThreadBottomContainer);
			
			// Main post(center) panel
			pnlPostContainer = new JPanel(new BorderLayout(5, 5));
			
			// Contains bottom post tools
			pnlPostBottomContainer = new JPanel(new BorderLayout(5, 5));
			pnlPostBottomContainer.setConstraints("South");
			
			// Post paging bar
			postPaging = new PagingBar(loadPostPage, true, true, true, true, true, false);
			postPaging.setConstraints("Center");			
			
			// New post button
			btnNewPost = new JButton("Reply");
			btnNewPost.setConstraints("East");
			
			pnlPostBottomContainer.appendAll(postPaging, btnNewPost);
			
			// Contains all of the posts
			pnlPostItemContainer = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 5));					
			pnlPostItemContainer.append(new JLabel("Select a thread to read it"));
			
			// Post scroll pane
			scrollPosts = new JScrollPane(new JViewport(pnlPostItemContainer, true), JScrollPane.SCROLLBAR_ALWAYS, JScrollPane.SCROLLBAR_NEVER);
			(scrollPosts.getViewport() as JViewport).setVerticalAlignment(AsWingConstants.TOP);
			scrollPosts.setConstraints("Center");
			
			// New post reply box
			pnlNewPost = new JPanel(new SoftBoxLayout(AsWingConstants.VERTICAL));
			pnlNewPost.setVisible(false);
			pnlNewPost.setConstraints("North");
			
			var lblNewPostMessage: JLabel = new JLabel("Your reply", null, AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(lblNewPostMessage, "Form.label");

			txtNewPostMessage = new JTextArea("", 15);
			GameLookAndFeel.changeClass(txtNewPostMessage, "Message");
			txtNewPostMessage.setWordWrap(true);
			txtNewPostMessage.setMaxChars(30000);
			
			var scrollMessage: JScrollPane = new JScrollPane(txtNewPostMessage);
			
			btnNewPostSubmit = new JButton("Post Reply");
			
			btnNewPostCancel = new JLabelButton("cancel");
			
			var pnlNewPostFooter: JPanel = new JPanel(new FlowLayout(AsWingConstants.RIGHT));
			pnlNewPostFooter.appendAll(btnNewPostCancel, btnNewPostSubmit);
			
			pnlNewPost.appendAll(lblNewPostMessage, scrollMessage, pnlNewPostFooter);
			
			// Append to post container
			pnlPostContainer.appendAll(pnlNewPost, scrollPosts, pnlPostBottomContainer);
			
			// Add to main layout
			pnlPostContainer.setConstraints("Center");
			pnlThreadContainer.setConstraints("West");			
			appendAll(pnlThreadContainer, pnlPostContainer);
			
			clearPostView();
		}
		
		public function createRefreshPanel(): JPanel {
			pnlNewMessage =  new JPanel(new SoftBoxLayout(SoftBoxLayout.X_AXIS, 5, SoftBoxLayout.CENTER));
			
			var lbl: JLabel = new JLabel("There is a new tribe message!");
			
			var btn: JLabelButton = new JLabelButton("Refresh");
			btn.addActionListener(function(e: Event = null) : void {
				remove(pnlNewMessage);
				clearPostView();
				lastThreadId = -1;				
				threadPaging.refreshPage(0);
			});

			GameLookAndFeel.changeClass(lbl, "darkHeader");
			pnlNewMessage.appendAll(lbl,btn);
			pnlNewMessage.setConstraints("North");
			return pnlNewMessage;
		}
		
		public function showRefreshButton(): void{
			append(createRefreshPanel());
		}
        
        public function afterCreate(createDlg: * , newThreadId: int):void {
            createDlg.getFrame().dispose();
            
            clearPostView();
            lastThreadId = newThreadId;
            threadPaging.refreshPage(0);
            
            Global.mapComm.MessageBoard.view(postLoader, 0, newThreadId);				
        }			       
	}

}