package src.UI.Dialog {
    import flash.events.Event;

    import org.aswing.*;
    import org.aswing.geom.*;

    import src.Comm.GameURLLoader;
    import src.Global;
    import src.UI.GameJPanel;
    import src.UI.LookAndFeel.GameLookAndFeel;

    public class MessageBoardThreadCreateDialog extends GameJPanel {
		
		private var pnlSubject:JPanel;
		private var lblSubject:JLabel;
		private var txtSubject:JTextField;
		private var pnlMessage:JPanel;
		private var lblMessage:JLabel;
		private var txtMessage:JTextArea;
		private var pnlFooter:JPanel;
		private var btnCreate:JButton;

		public function MessageBoardThreadCreateDialog(onAccept: Function, editThreadId: int = -1, subject: String = "", message: String = "")
		{			
			createUI();				

			title = editThreadId > 0 ? "Edit Thread" : "New Thread";
            
            txtSubject.setText(subject);
            txtMessage.setText(message);
			
			var self: MessageBoardThreadCreateDialog = this;
			btnCreate.addActionListener(function():void {

				if (txtSubject.getLength() == 0) {
					InfoDialog.showMessageDialog("Error", "Please enter a subject.");
					return;
				}
 
				if (txtMessage.getLength() == 0) {
					InfoDialog.showMessageDialog("Error", "Please write a message.");
					return;
				}				

				var messageLoader: GameURLLoader = new GameURLLoader();
				messageLoader.addEventListener(Event.COMPLETE, function(event: Event) : void {
					var data: Object;
					try
					{
						data = messageLoader.getDataAsObject();
					}
					catch (e: Error) {
						InfoDialog.showMessageDialog("Error", "Unable to send message. Try again later.");
						return;
					}

					if (data.error != null && data.error != "") {
						InfoDialog.showMessageDialog("Info", data.error);
						return;
					}										
					
					if (onAccept != null) 
						onAccept(self, data.id);
				});

				var message: * = getMessage();
				
				Global.mapComm.MessageBoard.addThread(messageLoader, editThreadId, message.subject, message.message);
			});
		}

		public function getMessage(): * {
			var message: * = new Object();
			message.subject = txtSubject.getText();
			message.message = txtMessage.getText();

			return message;
		}

		public function show(owner:* = null, modal:Boolean = true, onClose:Function = null):JFrame
		{
			super.showSelf(owner, modal, onClose);

			Global.gameContainer.showFrame(frame);

			txtSubject.requestFocus();
			
			return frame;
		}

		private function createUI(): void {
			//component creation
			setSize(new IntDimension(400, 256));
			var layout0:SoftBoxLayout = new SoftBoxLayout();
			layout0.setAxis(AsWingConstants.VERTICAL);
			layout0.setAlign(AsWingConstants.LEFT);
			layout0.setGap(5);
			setLayout(layout0);

			pnlSubject = new JPanel();
			var layout2:FlowLayout = new FlowLayout();
			layout2.setAlignment(AsWingConstants.LEFT);
			layout2.setMargin(false);
			pnlSubject.setLayout(layout2);

			lblSubject = new JLabel();
			lblSubject.setPreferredSize(new IntDimension(45, 25));
			lblSubject.setText("Subject");
			lblSubject.setHorizontalAlignment(AsWingConstants.RIGHT);
			GameLookAndFeel.changeClass(lblSubject, "Form.label");

			txtSubject = new JTextField();
			txtSubject.setMaxChars(60);
			txtSubject.setPreferredSize(new IntDimension(355, 25));

			pnlMessage = new JPanel();
			var layout3:SoftBoxLayout = new SoftBoxLayout();
			layout3.setAxis(AsWingConstants.VERTICAL);
			layout3.setAlign(AsWingConstants.LEFT);
			layout3.setGap(0);
			pnlMessage.setLayout(layout3);

			lblMessage = new JLabel();
			lblMessage.setText("Message");
			lblMessage.setHorizontalAlignment(AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(lblMessage, "Form.label");

			var scrollMessage: JScrollPane = new JScrollPane();
			scrollMessage.setPreferredSize(new IntDimension(400, 200));

			txtMessage = new JTextArea();
			txtMessage.setWordWrap(true);
			txtMessage.setMaxChars(30000);

			pnlFooter = new JPanel();
			var layout4:FlowLayout = new FlowLayout();
			layout4.setAlignment(AsWingConstants.CENTER);
			pnlFooter.setLayout(layout4);

			btnCreate = new JButton();
			btnCreate.setText("Post");

			//component layoution
			append(pnlSubject);
			append(pnlMessage);
			append(pnlFooter);

			pnlSubject.append(lblSubject);
			pnlSubject.append(txtSubject);

			pnlMessage.append(lblMessage);
			pnlMessage.append(scrollMessage);

			scrollMessage.append(txtMessage);

			pnlFooter.append(btnCreate);

		}
	}

}

