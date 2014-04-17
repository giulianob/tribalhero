package src.UI.Dialog{

    import org.aswing.*;
    import org.aswing.border.*;
    import org.aswing.ext.*;
    import org.aswing.geom.*;

    import src.Global;
    import src.Objects.Effects.RequirementFormula;
    import src.UI.GameJPanel;

    public class CreateTribeDialog extends GameJPanel {

		private var txtName: JTextField;
		private var btnOk: JButton;
		
		public function CreateTribeDialog(onAccept: Function) {
			createUI();

			var self: CreateTribeDialog = this;
			btnOk.addActionListener(function():void {				
				if (onAccept != null) onAccept(self);
			});
		}

		public function getTribeName():String{
			return txtName.getText();
		}

		public function show(owner:* = null, modal:Boolean = true, onClose: Function = null) :JFrame
		{
			super.showSelf(owner, modal, onClose);
			Global.gameContainer.showFrame(frame);

			return frame;
		}

		private function createUI():void {
			title = "Tribe";
			
			setPreferredWidth(350);
			setLayout(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 5));
			setConstraints("Center");
			setBorder(new EmptyBorder(null, new Insets(10, 10, 10, 10)));
			
			var form: Form = new Form();
			form.setVGap(5);

			var rowName: FormRow = new FormRow();
			rowName.setColumnChildrenIndecis("-1,0,1");

			var lblName: JLabel = new JLabel();
			lblName.setText("Tribe Name");
			lblName.setHorizontalAlignment(AsWingConstants.RIGHT);

			txtName = new JTextField();
			txtName.setMaxChars(20);
			txtName.setPreferredSize(new IntDimension(150, 25));

			var pnlButtonHolder: JPanel = new JPanel(new FlowLayout(AsWingConstants.CENTER));

			btnOk = new JButton();
			btnOk.setText("Create Tribe");

			var lblMessage: MultilineLabel;			
			if (!RequirementFormula.canCreateTribe()) {
				lblMessage = new MultilineLabel("You need to be part of a tribe before you can use this feature.\n\nYou can create your own tribe after you have a Town Center (Level 5) or you can join an existing tribe right now if someone invites you.", 6);
				
				append(lblMessage);			
			}
			else {
				lblMessage = new MultilineLabel("You need to be part of a tribe before you can use this feature.\n\nYou can join an existing tribe or create your own below by typing in a name below", 6);
				
				append(lblMessage);
				append(form);				
			}
		

			form.append(rowName);
			form.append(pnlButtonHolder);

			rowName.append(lblName);
			rowName.append(txtName);

			pnlButtonHolder.append(btnOk);
		}
	}
}

