package src.UI.Dialog {
	import flash.events.Event;
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

	public class MessageViewDialog extends GameJPanel {

		private var pnlName:JPanel;
		private var lblName:JLabel;
		private var txtName:JLabel;
		private var pnlDate:JPanel;
		private var lblDate:JLabel;
		private var txtDate:JLabel;
		private var pnlSubject:JPanel;
		private var lblSubject:JLabel;
		private var txtSubject:MultilineLabel;
		private var pnlMessage:JPanel;
		private var lblMessage:JLabel;
		private var scrollMessage:JScrollPane;
		private var txtMessage:MultilineLabel;
		private var pnlFooter:JPanel;
		private var btnDelete:JButton;

		private var message: *;

		public function MessageViewDialog(message: *, onDelete: Function)
		{
			createUI();

			this.message = message;

			txtName.setText(message.name);
			txtMessage.setText(message.message);
			txtSubject.setText(message.subject);
			txtDate.setText(message.date);

			if (message.isRecipient) {
				title = "Message from " + message.name;
				lblName.setText("From");
			} else {
				title = "Message to " + message.name;
				lblName.setText("To");
			}

			var self: MessageViewDialog = this;
			btnDelete.addActionListener(function(e: Event) : void {
				if (onDelete != null) onDelete(self);
			});
		}

		public function show(owner:* = null, modal:Boolean = true, onClose:Function = null):JFrame
		{
			super.showSelf(owner, modal, onClose);

			Global.gameContainer.showFrame(frame);

			return frame;
		}

		private function createUI(): void {
			//component creation
			var layout0:SoftBoxLayout = new SoftBoxLayout();
			layout0.setAxis(AsWingConstants.VERTICAL);
			layout0.setAlign(AsWingConstants.LEFT);
			layout0.setGap(0);
			setLayout(layout0);

			pnlName = new JPanel();
			var layout1:FlowLayout = new FlowLayout();
			layout1.setAlignment(AsWingConstants.LEFT);
			layout1.setMargin(false);
			pnlName.setLayout(layout1);

			lblName = new JLabel();
			lblName.setPreferredSize(new IntDimension(45, 21));
			lblName.setHorizontalAlignment(AsWingConstants.RIGHT);
			GameLookAndFeel.changeClass(lblName, "Form.label");

			txtName = new JLabel();

			pnlDate = new JPanel();
			var layout4:FlowLayout = new FlowLayout();
			layout4.setAlignment(AsWingConstants.LEFT);
			layout4.setMargin(false);
			pnlDate.setLayout(layout4);

			lblDate = new JLabel();
			lblDate.setPreferredSize(new IntDimension(45, 21));
			lblDate.setText("Date");
			lblDate.setHorizontalAlignment(AsWingConstants.RIGHT);
			GameLookAndFeel.changeClass(lblDate, "Form.label");

			txtDate = new JLabel();

			pnlSubject = new JPanel();
			var layout2:FlowLayout = new FlowLayout();
			layout2.setAlignment(AsWingConstants.LEFT);
			layout2.setMargin(false);
			pnlSubject.setLayout(layout2);

			lblSubject = new JLabel();
			lblSubject.setPreferredSize(new IntDimension(45, 21));
			lblSubject.setText("Subject");
			lblSubject.setHorizontalAlignment(AsWingConstants.RIGHT);
			lblSubject.setVerticalAlignment(AsWingConstants.CENTER);
			GameLookAndFeel.changeClass(lblSubject, "Form.label");

			txtSubject = new MultilineLabel();
			txtSubject.setColumns(44);

			pnlMessage = new JPanel();
			pnlMessage.setLocation(new IntPoint(0, 207));
			pnlMessage.setSize(new IntDimension(400, 167));
			var layout3:SoftBoxLayout = new SoftBoxLayout();
			layout3.setAxis(AsWingConstants.VERTICAL);
			layout3.setAlign(AsWingConstants.LEFT);
			layout3.setGap(0);
			pnlMessage.setLayout(layout3);

			lblMessage = new JLabel();
			lblMessage.setText("Message");
			lblMessage.setHorizontalAlignment(AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(lblMessage, "Form.label");

			scrollMessage = new JScrollPane();
			scrollMessage.setPreferredSize(new IntDimension(400, 250));

			txtMessage = new MultilineLabel();
			txtMessage.setColumns(50);

			pnlFooter = new JPanel();
			var layout5:FlowLayout = new FlowLayout();
			layout5.setAlignment(AsWingConstants.LEFT);
			pnlFooter.setLayout(layout5);

			btnDelete = new JButton();
			btnDelete.setText("Delete");

			//component layoution
			append(pnlName);
			append(pnlDate);
			append(pnlSubject);
			append(pnlMessage);
			append(pnlFooter);

			pnlName.append(lblName);
			pnlName.append(txtName);

			pnlDate.append(lblDate);
			pnlDate.append(txtDate);

			pnlSubject.append(lblSubject);
			pnlSubject.append(txtSubject);

			pnlMessage.append(scrollMessage);

			scrollMessage.append(txtMessage);

			pnlFooter.append(btnDelete);

		}
	}

}

