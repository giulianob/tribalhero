package src.UI.Dialog{

    import org.aswing.*;
    import org.aswing.border.*;
    import org.aswing.ext.*;
    import org.aswing.geom.*;

    import src.Constants;

    import src.Global;
    import src.UI.Components.AutoCompleteTextField;
    import src.UI.Components.SimpleTooltip;
    import src.UI.GameJPanel;
    import src.Util.ProfanityFilter;
    import src.Util.StringHelper;
    import src.Util.Util;

	public class InitialCityDialog extends GameJPanel {

		private var form:Form;
		private var rowName:FormRow;
		private var lblName:JLabel;
		private var txtName:JTextField;
		private var separator14:JSeparator;
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
        private var chkPlayMusic: JCheckBox;

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

            chkPlayMusic.addSelectionListener(function(): void {
                Global.musicPlayer.toggle(chkPlayMusic.isSelected());
            });
		}

		public function getCityName():String{
			return txtName.getText();
		}
		
		public function getLocationParameter(): * {
			var obj: Object = {};
			obj.method = cbbLocation.getSelectedIndex();
			obj.playerName = txtPlayer.getText();
			obj.playerHash = txtPlayerHash.getText();
			return obj;
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

			setLayout(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 5));
			setConstraints("Center");
			var border0:EmptyBorder = new EmptyBorder();
			border0.setTop(10);
			border0.setLeft(10);
			border0.setBottom(10);
			border0.setRight(10);
			setBorder(border0);

			form = new Form();
			form.setVGap(5);

			//  City name row
			rowName = new FormRow();
			rowName.setColumnChildrenIndecis("-1,0,1,2");

			lblName = new JLabel();
			lblName.setText("City Name");
			lblName.setHorizontalAlignment(AsWingConstants.RIGHT);

			txtName = new JTextField();
			txtName.setMaxChars(16);
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

			btnOk = new JButton();
			btnOk.setText("Create City");

			lblWelcome = new MultilineLabel("Welcome! Since this is your first time on this server, you have to choose a name for your first city");

            chkPlayMusic = new JCheckBox("Play sounds/music");
            chkPlayMusic.setSelected(!Constants.session.soundMuted);

			append(lblWelcome);
			append(form);

			form.append(rowName);
			form.append(rowLocation);
			form.append(rowPlayer);
			form.append(rowPlayerHash);
			form.append(separator14);
			form.append(AsWingUtils.createPaneToHold(btnOk, new FlowLayout(AsWingConstants.CENTER)));
			form.append(AsWingUtils.createPaneToHold(chkPlayMusic, new FlowLayout(AsWingConstants.LEFT)));

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
		}
	}
}

