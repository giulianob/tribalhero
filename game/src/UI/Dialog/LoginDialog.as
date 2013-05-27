package src.UI.Dialog{

	import src.Constants;
	import src.Global;
	import src.UI.GameJPanel;
	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.geom.*;
	import org.aswing.colorchooser.*;
	import org.aswing.ext.*;

	/**
	 * ConnectionDialog
	 */
	public class LoginDialog extends GameJPanel {

		//members define
		private var form42:Form;
		private var rowAddress:FormRow;
		private var lblAddress:JLabel;
		private var txtAddress:JTextField;
		private var separator14:JSeparator;
		private var formrow8:FormRow;
		private var lblUsername:JLabel;
		private var txtUsername:JTextField;
		private var formrow10:FormRow;
		private var lblPassword:JLabel;
		private var txtPassword:JTextField;
		private var panel16:JPanel;
		private var btnLogin:JButton;

		/**
		 * ConnectionDialog Constructor
		 */
		public function LoginDialog(onAccept: Function) {
			createUI();

			var self: LoginDialog = this;
			btnLogin.addActionListener(function():void{
				if (onAccept != null) onAccept(self);
			});
		}

		//_________getters_________
		public function getTxtAddress():JTextField{
			return txtAddress;
		}

		public function getTxtUsername():JTextField{
			return txtUsername;
		}

		public function getTxtPassword():JTextField{
			return txtPassword;
		}

		public function getBtnLogin():JButton{
			return btnLogin;
		}

		public function show(owner:* = null, modal:Boolean = true, onClose: Function = null) :JFrame
		{
			super.showSelf(owner, modal, onClose);
			frame.setClosable(false);
			Global.gameContainer.showFrame(frame);

			return frame;
		}

		private function createUI():void {
			title = "Login";
			//component creation
			//setSize(new IntDimension(250, 137));
			setConstraints("Center");

			form42 = new Form();
			//form42.setSize(new IntDimension(208, 117));
			form42.setVGap(5);

			rowAddress = new FormRow();
			rowAddress.setSize(new IntDimension(256, 25));
			rowAddress.setColumnChildrenIndecis("-1,0,1,2");

			lblAddress = new JLabel();
			lblAddress.setPreferredSize(new IntDimension(65, 25));
			lblAddress.setText("Address");
			lblAddress.setHorizontalAlignment(AsWingConstants.RIGHT);

			txtAddress = new JTextField();
			txtAddress.setSize(new IntDimension(150, 25));
			txtAddress.setPreferredSize(new IntDimension(150, 25));
			txtAddress.setText(Constants.hostname);

			separator14 = new JSeparator();
			separator14.setSize(new IntDimension(220, 2));

			formrow8 = new FormRow();
			formrow8.setSize(new IntDimension(256, 25));
			formrow8.setColumnChildrenIndecis("-1,0,1,2");

			lblUsername = new JLabel();
			lblUsername.setPreferredSize(new IntDimension(65, 25));
			lblUsername.setText("Username");
			lblUsername.setHorizontalAlignment(AsWingConstants.RIGHT);

			txtUsername = new JTextField();
			txtUsername.setSize(new IntDimension(150, 25));
			txtUsername.setPreferredSize(new IntDimension(150, 25));
			txtUsername.setText(Constants.username);

			formrow10 = new FormRow();
			formrow10.setSize(new IntDimension(194, 0));
			formrow10.setColumnChildrenIndecis("-1,0,1,2");

			lblPassword = new JLabel();
			lblPassword.setPreferredSize(new IntDimension(65, 25));
			lblPassword.setText("Password");
			lblPassword.setHorizontalAlignment(AsWingConstants.RIGHT);

			txtPassword = new JTextField();
			txtPassword.setSize(new IntDimension(150, 25));
			txtPassword.setDisplayAsPassword(true);

			panel16 = new JPanel();
			panel16.setSize(new IntDimension(10, 10));
			var layout1:FlowLayout = new FlowLayout();
			layout1.setAlignment(AsWingConstants.CENTER);
			panel16.setLayout(layout1);

			btnLogin = new JButton();
			btnLogin.setSize(new IntDimension(34, 22));
			btnLogin.setText("Login");

			//component layoution
			append(form42);

			form42.append(rowAddress);
			form42.append(separator14);
			form42.append(formrow8);
			form42.append(formrow10);
			form42.append(panel16);

			rowAddress.append(lblAddress);
			rowAddress.append(txtAddress);

			formrow8.append(lblUsername);
			formrow8.append(txtUsername);

			formrow10.append(lblPassword);
			formrow10.append(txtPassword);

			panel16.append(btnLogin);
		}
	}
}

