package src.UI.Dialog{

    import org.aswing.*;
    import org.aswing.border.*;
    import org.aswing.ext.*;
    import org.aswing.geom.*;

    import src.Constants;
    import src.Global;
    import src.UI.Components.TribeLabel;
    import src.UI.GameJPanel;
    import src.UI.LookAndFeel.GameLookAndFeel;

    public class TribeInviteRequestDialog extends GameJPanel {

		private var lblName:TribeLabel;
		private var lblNameTitle:JLabel;		
		private var btnAccept:JButton;
		private var btnDecline:JButton;
		private var lblWelcome: MultilineLabel;
		
		private var result: Boolean;

		public function TribeInviteRequestDialog(onAccept: Function) {
			createUI();


			
			var self: TribeInviteRequestDialog = this;
			btnAccept.addActionListener(function():void {				
				result = true;
				if (onAccept != null) onAccept(self);
			});
			btnDecline.addActionListener(function():void {				
				result = false;
				if (onAccept != null) onAccept(self);
			});
		}

		public function getResult():Boolean{
			return result;
		}

		public function show(owner:* = null, modal:Boolean = true, onClose: Function = null) :JFrame
		{
			super.showSelf(owner, modal, onClose);
			Global.gameContainer.showFrame(frame);

			return frame;
		}

		private function createUI():void {
			title = "Join a Tribe";
			//component creation
			setLayout(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 5));
			setPreferredWidth(250);			
			var border0:EmptyBorder = new EmptyBorder();
			border0.setTop(10);
			border0.setLeft(10);
			border0.setBottom(10);
			border0.setRight(10);
			setBorder(border0);

			var pnlName: JPanel = new JPanel(new FlowLayout(AsWingConstants.CENTER));
			
			lblNameTitle = new JLabel();
			lblNameTitle.setLocation(new IntPoint(0, 2));
			lblNameTitle.setSize(new IntDimension(52, 25));
			lblNameTitle.setText("Tribe Name");
			lblNameTitle.setHorizontalAlignment(AsWingConstants.RIGHT);

			lblName = new TribeLabel(Constants.session.tribeInviteId);
			GameLookAndFeel.changeClass(lblName, "Form.label");
			lblName.setHorizontalAlignment(AsWingConstants.LEFT);

			var pnlButtons: JPanel = new JPanel(new FlowLayout(AsWingConstants.CENTER));

			btnDecline = new JButton();
			btnDecline.setLocation(new IntPoint(87, 5));
			btnDecline.setSize(new IntDimension(34, 22));
			btnDecline.setText("Decline");
			
			btnAccept = new JButton();
			btnAccept.setLocation(new IntPoint(87, 5));
			btnAccept.setSize(new IntDimension(34, 22));
			btnAccept.setText("Accept");

			lblWelcome = new MultilineLabel("You have been invited to a tribe. You may accept or decline this request below.");

			//component layout
			append(lblWelcome);
			append(pnlName);
			append(pnlButtons);
			
			pnlName.append(lblNameTitle);
			pnlName.append(lblName);
			
			pnlButtons.append(btnAccept);			
			pnlButtons.append(btnDecline);			
		}
	}
}

