package src.UI.Dialog{

import src.Constants;
import src.Global;
import src.UI.GameJPanel;
import org.aswing.*;
import org.aswing.border.*;
import org.aswing.geom.*;
import org.aswing.colorchooser.*;
import org.aswing.ext.*;
import src.Util.Util;

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
		setSize(new IntDimension(228, 137));
		setConstraints("Center");
		var border0:EmptyBorder = new EmptyBorder();
		border0.setTop(10);
		border0.setLeft(10);
		border0.setBottom(10);
		border0.setRight(10);
		setBorder(border0);
		
		form42 = new Form();
		form42.setLocation(new IntPoint(10, 10));
		form42.setSize(new IntDimension(208, 117));
		form42.setVGap(5);
		
		rowAddress = new FormRow();
		rowAddress.setLocation(new IntPoint(0, 42));
		rowAddress.setSize(new IntDimension(256, 21));
		rowAddress.setColumnChildrenIndecis("-1,0,1,2");
		
		lblAddress = new JLabel();
		lblAddress.setLocation(new IntPoint(0, 2));
		lblAddress.setSize(new IntDimension(52, 17));
		lblAddress.setText("Address");
		lblAddress.setHorizontalAlignment(AsWingConstants.RIGHT);
		
		txtAddress = new JTextField();
		txtAddress.setLocation(new IntPoint(74, 20));
		txtAddress.setSize(new IntDimension(150, 21));
		txtAddress.setPreferredSize(new IntDimension(150, 21));
		txtAddress.setText(Constants.hostname);
		
		separator14 = new JSeparator();
		separator14.setLocation(new IntPoint(10, 33));
		separator14.setSize(new IntDimension(206, 2));
		
		formrow8 = new FormRow();
		formrow8.setLocation(new IntPoint(0, 23));
		formrow8.setSize(new IntDimension(256, 21));
		formrow8.setColumnChildrenIndecis("-1,0,1,2");
		
		lblUsername = new JLabel();
		lblUsername.setLocation(new IntPoint(13, 51));
		lblUsername.setSize(new IntDimension(52, 17));
		lblUsername.setText("Username");
		lblUsername.setHorizontalAlignment(AsWingConstants.RIGHT);
		
		txtUsername = new JTextField();
		txtUsername.setLocation(new IntPoint(79, 51));
		txtUsername.setSize(new IntDimension(150, 21));
		txtUsername.setPreferredSize(new IntDimension(150, 21));
		txtUsername.setText(Constants.username);
		
		formrow10 = new FormRow();
		formrow10.setLocation(new IntPoint(10, 56));
		formrow10.setSize(new IntDimension(194, 0));
		formrow10.setColumnChildrenIndecis("-1,0,1,2");
		
		lblPassword = new JLabel();
		lblPassword.setLocation(new IntPoint(2, 0));
		lblPassword.setSize(new IntDimension(52, 17));
		lblPassword.setText("Password");
		lblPassword.setHorizontalAlignment(AsWingConstants.RIGHT);
		
		txtPassword = new JTextField();
		txtPassword.setLocation(new IntPoint(56, 0));
		txtPassword.setSize(new IntDimension(150, 21));
		txtPassword.setDisplayAsPassword(true);
		
		panel16 = new JPanel();
		panel16.setLocation(new IntPoint(208, 0));
		panel16.setSize(new IntDimension(10, 10));
		var layout1:FlowLayout = new FlowLayout();
		layout1.setAlignment(AsWingConstants.CENTER);
		panel16.setLayout(layout1);
		
		btnLogin = new JButton();
		btnLogin.setLocation(new IntPoint(87, 5));
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
