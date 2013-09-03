package src.UI.Dialog{

	import com.junkbyte.console.core.CcCallbackDispatcher;
	import org.aswing.util.StringUtils;
	import src.Global;
	import src.UI.Components.AutoCompleteTextField;
	import src.UI.Components.SimpleTooltip;
	import src.UI.GameJPanel;
	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.geom.*;
	import org.aswing.colorchooser.*;
	import org.aswing.ext.*;
	import src.Util.ProfanityFilter;
	import src.Util.StringHelper;
	import src.Util.Util;

	/**
	 * ConnectionDialog
	 */
	public class InitialCityDialog extends GameJPanel {

		//members define
		private var form42:Form;
		private var rowName:FormRow;
		private var lblName:JLabel;
		private var txtName:JTextField;
		private var separator14:JSeparator;
		private var panel16:JPanel;
		private var btnOk:JButton;
		private var lblWelcome: MultilineLabel;
		
		private var rowLocation: FormRow;
		private var lblLocation: JLabel;
		private var cbbLocation: JComboBox;
		
		private var rowPlayer: FormRow;
		private var lblPlayer: JLabel;
		private var txtPlayer: AutoCompleteTextField;
		
		private var rowPlayerHash: FormRow;
		private var lblPlayerHash: JLabel;
		private var txtPlayerHash: JTextField;

		/**
		 * InitialCityDialog Constructor
		 */
		public function InitialCityDialog(onAccept: Function) {
			createUI();

			var self: InitialCityDialog = this;
			btnOk.addActionListener(function():void {				
				if (new ProfanityFilter().quickValidate(getCityName()) == false) {
					InfoDialog.showMessageDialog("Oops", "It looks like you have entered an invalid city name. This may be because the name you have chosen is blacklisted.");
					return;
				}			
				
				if (onAccept != null) onAccept(self);
			});
		}

		//_________getters_________
		public function getCityName():String{
			return txtName.getText();
		}
		
		public function getLocationParameter(): * {
			var obj: Object = new Object();
			obj.method = cbbLocation.getSelectedIndex();
			obj.playerName = txtPlayer.getText();
			obj.playerHash = txtPlayerHash.getText();
			return obj;
		}
		
		public function getPlayerHash(): String {
			return txtPlayerHash.getText();
		}

		public function show(owner:* = null, modal:Boolean = true, onClose: Function = null) :JFrame
		{
			super.showSelf(owner, modal, onClose);
			frame.setClosable(false);
			Global.gameContainer.showFrame(frame);

			return frame;
		}

		private function createUI():void {
			title = "Create Your City";
			//component creation
			setLayout(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 5));
			setSize(new IntDimension(228, 237));
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

			//  City name row
			rowName = new FormRow();
			rowName.setLocation(new IntPoint(0, 42));
			rowName.setSize(new IntDimension(256, 25));
			rowName.setColumnChildrenIndecis("-1,0,1,2");

			lblName = new JLabel();
			lblName.setLocation(new IntPoint(0, 2));
			lblName.setSize(new IntDimension(52, 25));
			lblName.setText("City Name");
			lblName.setHorizontalAlignment(AsWingConstants.RIGHT);

			txtName = new JTextField();
			txtName.setMaxChars(16);
			txtName.setLocation(new IntPoint(74, 20));
			txtName.setSize(new IntDimension(150,25));
			txtName.setPreferredSize(new IntDimension(150, 25));
			
			// Location row
			rowLocation = new FormRow();
			rowLocation.setColumnChildrenIndecis("-1,0,1,2");
			lblLocation = new JLabel("Location");
			lblLocation.setHorizontalAlignment(AsWingConstants.RIGHT);
			cbbLocation = new JComboBox( [ "Next available", "Close to a friend" ] );
			cbbLocation.setSelectedIndex(0);
			cbbLocation.addActionListener(function(): void {
				rowPlayer.setVisible(cbbLocation.getSelectedIndex()==1);
				rowPlayerHash.setVisible(cbbLocation.getSelectedIndex() == 1);
				
				if (getFrame()) {
					getFrame().pack();
					Util.centerFrame(getFrame());
				}
			});
			
			// Player row
			rowPlayer = new FormRow();
			rowPlayer.setColumnChildrenIndecis("-1,0,1,2");
			lblPlayer = new JLabel("Player name");
			lblPlayer.setHorizontalAlignment(AsWingConstants.RIGHT);
			txtPlayer = new AutoCompleteTextField(Global.mapComm.General.autoCompletePlayer);

			// Player hash row
			rowPlayerHash = new FormRow();
			rowPlayerHash.setColumnChildrenIndecis("-1,0,1,2");
			lblPlayerHash = new JLabel("Invitation Code");
			lblPlayerHash.setHorizontalAlignment(AsWingConstants.RIGHT);
			txtPlayerHash = new JTextField();
			new SimpleTooltip(txtPlayerHash, StringHelper.localize("PLAYER_PROFILE_INVITATION_TOOLTIP_LOCATION"));
			
			separator14 = new JSeparator();
			separator14.setLocation(new IntPoint(10, 33));
			separator14.setSize(new IntDimension(206, 2));

			panel16 = new JPanel();
			panel16.setLocation(new IntPoint(208, 0));
			panel16.setSize(new IntDimension(10, 10));
			var layout1:FlowLayout = new FlowLayout();
			layout1.setAlignment(AsWingConstants.CENTER);
			panel16.setLayout(layout1);

			btnOk = new JButton();
			btnOk.setLocation(new IntPoint(87, 5));
			btnOk.setSize(new IntDimension(34, 22));
			btnOk.setText("Create City");

			lblWelcome = new MultilineLabel("Welcome! Since this is your first time on this server, you have to choose a name for your first city");

			//component layoution
			append(lblWelcome);
			append(form42);

			form42.append(rowName);
			form42.append(rowLocation);
			form42.append(rowPlayer);
			form42.append(rowPlayerHash);
			form42.append(separator14);
			form42.append(panel16);
			
			rowLocation.append(lblLocation);
			rowLocation.append(cbbLocation);
			
			rowName.append(lblName);
			rowName.append(txtName);
			
			rowPlayer.append(lblPlayer);
			rowPlayer.append(txtPlayer);
			rowPlayer.setVisible(false);
			
			rowPlayerHash.append(lblPlayerHash);
			rowPlayerHash.append(txtPlayerHash);
			rowPlayerHash.setVisible(false);
			
			panel16.append(btnOk);
		}
	}
}

