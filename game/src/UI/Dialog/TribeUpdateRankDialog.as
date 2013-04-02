package src.UI.Dialog{

	import com.google.analytics.ecommerce.Item;
	import fl.lang.Locale;
    import flash.events.Event;
    import org.aswing.event.AWEvent;
    import org.aswing.event.InteractiveEvent;
	import src.Constants;
	import src.Global;
	import src.Objects.Tribe;
	import src.UI.GameJPanel;
	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.geom.*;
	import org.aswing.colorchooser.*;
	import org.aswing.ext.*;
	import src.UI.LookAndFeel.GameLookAndFeel;

	public class TribeUpdateRankDialog extends GameJPanel {
		
		private var permissions : Array = [
			new JCheckBox("Invite Tribesman"),
			new JCheckBox("Kick Tribesman"),
			new JCheckBox("Set Tribesman Rank"),
			new JCheckBox("Repair Stronghold Gate"),
			new JCheckBox("Upgrade Tribe"),
			new JCheckBox("Create Assignments"),
			new JCheckBox("Delete Message Board Posts")
		];
		
		private var comboRankId: JComboBox;
		private var txtRankName: JTextField;
		private var rightsPanel : JPanel;
		private var currentId: int = 0;
        private var btnSave:JButton;
        private var btnCancel:JButton;
        private var btnEdit:JButton;
        private var pnlRanks: JPanel;
        private var pnlEditor: JPanel;
		
		public function TribeUpdateRankDialog() {
			createUI();			
			
			btnSave.addActionListener(function() :void {
				onSave();
			});            
            
            btnCancel.addActionListener(function():void {
                showEditor(false);
            });
            
            btnEdit.addActionListener(function():void {
                var rank :* = Constants.tribe.ranks[comboRankId.getSelectedIndex()];
                txtRankName.setText(rank.name);
                setPermissions(rank.rights);                
                
                showEditor(true);
            });
		}
        
        private function showEditor(show: Boolean) {
            pnlEditor.setVisible(show);
            pnlRanks.setVisible(!show);
        }
		
		private function setPermissions(value: int) : void {
			for (var i:int = 0; i < this.permissions.length; ++i) {
				var checkbox: JCheckBox = this.permissions[i];
				if (Tribe.ALL & value) {
					checkbox.setSelected(true);
					checkbox.setEnabled(false);
				} else if ((1 << (i+1)) & value) {
					checkbox.setSelected(true);
					checkbox.setEnabled(true);
				} else {
					checkbox.setSelected(false);
					checkbox.setEnabled(true);
				}
			}
		}
		
		private function getPermissions(): int {
			var value:int = 0;
			for (var i:int = 0; i < this.permissions.length; ++i) {
				var checkbox: JCheckBox = this.permissions[i];
				if (checkbox.isSelected()) {
					value += 1 << (i + 1);
				}
			}
			return value;
		}

		public function show(owner:* = null, modal:Boolean = true, onClose: Function = null) :JFrame
		{
			super.showSelf(owner, modal, onClose);
			Global.gameContainer.showFrame(frame);

			return frame;
		}
		
		private function onSaveComplete(rank: *) : void {
			InfoDialog.showMessageDialog("Info", Locale.loadString("TRIBE_RANK_UPDATED"), function():void {
				update();
			});
		}
		
		private function onSave() : void {
			currentId = comboRankId.getSelectedIndex();
			Global.mapComm.Tribe.updateRank(currentId, txtRankName.getText(), getPermissions(),onSaveComplete);
		}
		
		private function update(): void {
			var rankList: Array = new Array();
			for ( var i:int = 0; i < Constants.tribe.ranks.length; ++i) {
				rankList.push((i + 1) + " - " + Constants.tribe.ranks[i].name);
			}
			comboRankId.setListData(rankList);
			
			comboRankId.setSelectedIndex(currentId);
            
            showEditor(false);
		}
		
		private function createUI():void {        
			title = "Update Tribe Rank";
						
			setLayout(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 20));
			
            pnlRanks = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 10));
            {
                comboRankId = new JComboBox();
                comboRankId.setPreferredWidth(175);
            
                btnEdit = new JButton("Edit");
                
                pnlRanks.append(comboRankId);
                pnlRanks.append(AsWingUtils.createPaneToHold(btnEdit, new FlowLayout()));
            }


			pnlEditor = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 10));
            {
                txtRankName = new JTextField();
                txtRankName.setPreferredWidth(175);
                rightsPanel = new JPanel(new GridLayout(0, 3, 10 , 10));
                
                for each( var checkbox:JCheckBox in permissions) {
                    checkbox.setHorizontalAlignment(AsWingConstants.LEFT);
                    rightsPanel.append(checkbox);
                }
                
                var form:Form = new Form();
                form.setHGap(20);
                form.setVGap(20);
                
                form.addRow(new JLabel("Rank"), AsWingUtils.createPaneToHold(comboRankId, new FlowLayout()));
                form.addRow(new JLabel("Rank Name"), AsWingUtils.createPaneToHold(txtRankName, new FlowLayout()));
                form.addRow(new JLabel("Permissions"), rightsPanel);
      
                btnSave = new JButton("Save");                       
                
                var pnlButtons: JPanel = new JPanel(new FlowLayout(AsWingConstants.CENTER));
                pnlButtons.appendAll(btnSave);
                
                pnlEditor.append(form);			
                pnlEditor.append(pnlButtons);
            }
            
            appendAll(pnlRanks, pnlEditor);
            
			update();
		}
	}
}
