package src.UI.Dialog{

	import com.google.analytics.ecommerce.Item;
	import fl.lang.Locale;
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
			new JCheckBox("Invite"),
			new JCheckBox("Kick"),
			new JCheckBox("Set Rank"),
			new JCheckBox("Repair"),
			new JCheckBox("Upgrade"),
			new JCheckBox("Assignment Create"),
			new JCheckBox("Delete Post")
		];
		
		private var comboRankId: JComboBox;
		private var txtRankName: JTextField;
		private var rightsPanel : JPanel;
		private var currentId: int = 0;
		
		public function TribeUpdateRankDialog() {
			createUI();			
			
		}
		
		private function setPermissions(value: int) : void {
			for (var i:int = 0; i < this.permissions.length; ++i) {
				var checkbox: JCheckBox = this.permissions[i];
				if ( Tribe.ALL & value) {
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
		
		private function onChangeRankId() :void {
			var rank :* = Constants.tribe.ranks[comboRankId.getSelectedIndex()];
			txtRankName.setText(rank.name);
			setPermissions(rank.rights);
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
		}
		
		private function createUI():void {
			title = "Update Tribe Rank";
			
			var border0:EmptyBorder = new EmptyBorder();
			setLayout(new SoftBoxLayout(SoftBoxLayout.Y_AXIS,20));
			border0.setTop(20);
			border0.setLeft(20);
			border0.setBottom(20);
			border0.setRight(20);
			setBorder(border0);
			
			comboRankId = new JComboBox();
			comboRankId.addActionListener(function():void {
				onChangeRankId();
			});
			
			txtRankName = new JTextField();
			rightsPanel = new JPanel(new GridLayout(0, 3, 10 , 10));
			
			for each( var checkbox:JCheckBox in permissions) {
				checkbox.setHorizontalAlignment(AsWingConstants.LEFT);
				rightsPanel.append(checkbox);
			}
			
			var form:Form = new Form();
			form.setHGap(20);
			form.setVGap(20);
			
			form.addRow(new JLabel("Rank"), comboRankId);
			form.addRow(new JLabel("Rank Name"), txtRankName);
			form.addRow(new JLabel("Permissions"), rightsPanel);

			append(form);
			var btnSave: JButton = new JButton("Save");
			btnSave.setWidth(80);
			btnSave.addActionListener(function() :void {
				onSave();
			});
			append(btnSave);
			update();
		}
	}
}
