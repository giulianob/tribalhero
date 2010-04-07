package src.UI.Dialog {
	import src.Global;
	import src.Map.*;
	import src.Objects.*;
	import src.UI.GameJPanel;
	import src.Objects.Troop.*;
	import src.UI.GameLookAndFeel;

	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.geom.*;
	import org.aswing.colorchooser.*;
	import org.aswing.ext.*;

	public class MessageCreateDialog extends GameJPanel {

		private var pnlTo:JPanel;
		private var lblTo:JLabel;
		private var txtTo:JTextField;
		private var pnlSubject:JPanel;
		private var lblSubject:JLabel;
		private var txtSubject:JTextField;
		private var pnlMessage:JPanel;
		private var lblMessage:JLabel;
		private var txtMessage:JTextArea;
		private var pnlFooter:JPanel;
		private var btnSend:JButton;

		public function MessageCreateDialog(onAccept: Function, to: String = "")
		{
			createUI();

			txtTo.setText(to);

			title = "New Message";

			var self: MessageCreateDialog = this;
			btnSend.addActionListener(function():void {
				
				if (txtTo.getLength() == 0) {
					InfoDialog.showMessageDialog("Error", "Please specify a recipient.");
					return;
				}				
				
				if (txtSubject.getLength() < 3) {
					InfoDialog.showMessageDialog("Error", "Subject must be at least 3 characters long.");
					return;
				}
				
				if (txtMessage.getLength() == 0) {
					InfoDialog.showMessageDialog("Error", "Please enter a message.");
					return;
				}								
				
				if (onAccept != null) onAccept(self);
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
			lblTo.setPreferredSize(new IntDimension(45, 21));
			lblTo.setText("To");
			lblTo.setHorizontalAlignment(AsWingConstants.RIGHT);
			GameLookAndFeel.changeClass(lblTo, "Form.label");

			txtTo = new JTextField();
			txtTo.setPreferredSize(new IntDimension(100, 21));

			pnlSubject = new JPanel();
			var layout2:FlowLayout = new FlowLayout();
			layout2.setAlignment(AsWingConstants.LEFT);
			layout2.setMargin(false);
			pnlSubject.setLayout(layout2);

			lblSubject = new JLabel();
			lblSubject.setPreferredSize(new IntDimension(45, 21));
			lblSubject.setText("Subject");
			lblSubject.setHorizontalAlignment(AsWingConstants.RIGHT);
			GameLookAndFeel.changeClass(lblSubject, "Form.label");

			txtSubject = new JTextField();
			txtSubject.setMaxChars(60);
			txtSubject.setPreferredSize(new IntDimension(355, 21));

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
			txtMessage.setMaxChars(950);

			pnlFooter = new JPanel();
			var layout4:FlowLayout = new FlowLayout();
			layout4.setAlignment(AsWingConstants.CENTER);
			pnlFooter.setLayout(layout4);

			btnSend = new JButton();
			btnSend.setText("Send");

			//component layoution
			append(pnlTo);
			append(pnlSubject);
			append(pnlMessage);
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

