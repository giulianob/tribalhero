package src.UI.Dialog {
    import flash.events.Event;

    import org.aswing.*;
    import org.aswing.border.*;
    import org.aswing.ext.*;
    import org.aswing.geom.*;

    import src.Comm.GameURLLoader;
    import src.Global;
    import src.UI.Components.AutoCompleteTextField;
    import src.UI.GameJPanel;
    import src.UI.LookAndFeel.GameLookAndFeel;
    import src.UI.LookAndFeel.GamePanelBackgroundDecorator;

    public class MessageCreateDialog extends GameJPanel {

		private var pnlTo:JPanel;
		private var lblTo:JLabel;
		private var txtTo:JTextField;
		private var pnlSubject:JPanel;
		private var lblSubject:JLabel;
		private var txtSubject:JTextField;
		
		private var originalMsg: String;
		
		private var pnlMessage:JPanel;
		private var lblMessage:JLabel;
		private var txtMessage:JTextArea;
		
		private var pnlOrigMessage:JPanel;
		private var lblOrigMessage:JLabel;
		private var txtOrigMessage:JTextArea;		
		
		private var pnlFooter:JPanel;
		private var btnSend:JButton;

		public function MessageCreateDialog(onSent: Function, to: String = "", subject: String = "", isReply: Boolean = false, originalMsg: String = "")
		{
			this.originalMsg = originalMsg;
			
			createUI();				

			txtTo.setText(to);
			
			if (isReply && subject.substr(0, 3) != "Re:") {				
				subject = "Re: " + subject;
			}		
			
			txtSubject.setText(subject);

			title = "New Message";
			
			var self: MessageCreateDialog = this;
			btnSend.addActionListener(function():void {

				if (txtTo.getLength() == 0) {
					InfoDialog.showMessageDialog("Error", "Please specify a recipient.");
					return;
				}

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
					
					if (onSent != null) onSent(self);
				});

				var message: * = getMessage();
				Global.mapComm.Messaging.send(messageLoader, message.to, message.subject, message.message);				
			});
		}

		public function getMessage(): * {
			var message: * = new Object();
			message.subject = txtSubject.getText();
			message.message = txtMessage.getText();
			message.to = txtTo.getText();

			return message;
		}

		public function show(owner:* = null, modal:Boolean = true, onClose:Function = null):JFrame
		{
			super.showSelf(owner, modal, onClose);

			Global.gameContainer.showFrame(frame);

			// Set focus to subject field if the To is already filled
			if (txtSubject.getText() != "")
				txtMessage.requestFocus();			
			else if (txtTo.getText() != "")
				txtSubject.requestFocus();		
			else
				txtTo.requestFocus();
			
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

			pnlTo = new JPanel();
			var layout1:FlowLayout = new FlowLayout();
			layout1.setAlignment(AsWingConstants.LEFT);
			layout1.setMargin(false);
			pnlTo.setLayout(layout1);

			lblTo = new JLabel();
			lblTo.setPreferredSize(new IntDimension(45, 25));
			lblTo.setText("To");
			lblTo.setHorizontalAlignment(AsWingConstants.RIGHT);
			GameLookAndFeel.changeClass(lblTo, "Form.label");

			txtTo = new AutoCompleteTextField(Global.mapComm.General.autoCompletePlayer);
			txtTo.setPreferredSize(new IntDimension(100, 25));

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
			pnlMessage.setLayout(new SoftBoxLayout(AsWingConstants.VERTICAL, 0, AsWingConstants.LEFT));

			lblMessage = new JLabel("Message", null, AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(lblMessage, "Form.label");

			var scrollMessage: JScrollPane = new JScrollPane();
			scrollMessage.setPreferredSize(new IntDimension(400, 200));

			txtMessage = new JTextArea();
			GameLookAndFeel.changeClass(txtMessage, "Message");
			txtMessage.setWordWrap(true);
			txtMessage.setMaxChars(30000);

			pnlFooter = new JPanel();
			var layout4:FlowLayout = new FlowLayout();
			layout4.setAlignment(AsWingConstants.CENTER);
			pnlFooter.setLayout(layout4);

			btnSend = new JButton();
			btnSend.setText("Send");

			pnlOrigMessage = new JPanel(new SoftBoxLayout(AsWingConstants.VERTICAL, 0, AsWingConstants.LEFT));
			pnlOrigMessage.setBackgroundDecorator(new GamePanelBackgroundDecorator("TabbedPane.top.contentRoundImage"));
			pnlOrigMessage.setBorder(new EmptyBorder(null, UIManager.get("TabbedPane.contentMargin") as Insets));					
			
			lblOrigMessage = new JLabel("Original Message", null, AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(lblOrigMessage, "Form.label");

			var scrollOrigMessage: JScrollPane = new JScrollPane();
			scrollOrigMessage.setPreferredSize(new IntDimension(400, 120));

			txtOrigMessage = new MultilineLabel(originalMsg, 0, 50);			
			GameLookAndFeel.changeClass(txtOrigMessage, "Message");

			
			//component layoution
			append(pnlTo);
			append(pnlSubject);
			append(pnlMessage);
			
			if (originalMsg != "") {
				pnlOrigMessage.appendAll(lblOrigMessage, scrollOrigMessage);
				scrollOrigMessage.append(txtOrigMessage);				
				append(pnlOrigMessage);
			}						
			
			append(pnlFooter);

			pnlTo.append(lblTo);
			pnlTo.append(txtTo);

			pnlSubject.append(lblSubject);
			pnlSubject.append(txtSubject);

			pnlMessage.append(lblMessage);
			pnlMessage.append(scrollMessage);

			scrollMessage.append(txtMessage);		

			pnlFooter.append(btnSend);

		}
	}

}

