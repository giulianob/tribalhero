package src.UI.Dialog 
{
	import org.aswing.JLabel;
	import src.UI.GameJPanel;
	import src.Global;
	import src.Objects.Troop.TroopStub;
	import org.aswing.*;
	import org.aswing.geom.*;
	import src.UI.Components.SimpleTooltip;
	/**
	 * ...
	 * @author Anthony Lam
	 */
	public class TroopAttackModeDialog extends GameJPanel
	{
		protected var pnlAttackStrength:JPanel;
		protected var lblAttackStrength:JLabel;
		protected var rdAssault:JRadioButton;
		protected var rdRaid:JRadioButton;
		protected var rdSlaughter:JRadioButton;
		protected var pnlButton:JPanel;
		protected var btnOk:JButton;
		protected var radioGroup: ButtonGroup;
		
		protected var troopStub: TroopStub;

		public function TroopAttackModeDialog(troopStub : TroopStub) 
		{
			this.title = "Change attack strength";	
			this.troopStub = troopStub;
			createUI();
			switch(troopStub.attackMode) {
				case TroopStub.ATTACK_MODE_WEAK:
					rdRaid.setSelected(true);
					break;
				case TroopStub.ATTACK_MODE_NORMAL:
					rdAssault.setSelected(true);
					break;
				default:
					rdSlaughter.setSelected(true);
			}
		}
		
		public function show(owner:* = null, modal:Boolean = true, onClose: Function = null) :JFrame
		{
			super.showSelf(owner, modal, onClose);
			Global.gameContainer.showFrame(frame);
			return frame;
		}
		
		public function getMode(): int
		{
			if (rdAssault.isSelected()) return 1;
			else if (rdRaid.isSelected()) return 0;
			else if (rdSlaughter.isSelected()) return 2;
			else return -1;
		}
			
		public function createUI(): void 
		{
			setLayout(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 10));

			pnlAttackStrength = new JPanel(new SoftBoxLayout(SoftBoxLayout.X_AXIS, 5, AsWingConstants.LEFT));

			lblAttackStrength = new JLabel();
			lblAttackStrength.setText("Attack Strength:");

			rdAssault = new JRadioButton("Assault");
			rdAssault.setSelected(true);
			rdAssault.setHorizontalAlignment(AsWingConstants.LEFT);
			new SimpleTooltip(rdAssault, "Retreat if only 1/3 of the units remain");

			rdRaid = new JRadioButton("Raid");
			rdRaid.setHorizontalAlignment(AsWingConstants.LEFT);
			new SimpleTooltip(rdRaid, "Retreat if only 2/3 of the units remain");

			rdSlaughter = new JRadioButton("Slaughter");
			rdSlaughter.setHorizontalAlignment(AsWingConstants.LEFT);
			new SimpleTooltip(rdSlaughter, "Fight until death");

			pnlButton = new JPanel(new FlowLayout(AsWingConstants.CENTER));

			btnOk = new JButton();
			btnOk.setText("Ok");
			btnOk.addActionListener(function():void {
				Global.mapComm.Troop.switchAttackMode(troopStub, getMode());
				getFrame().dispose();
			});
			

			radioGroup = new ButtonGroup();
			radioGroup.appendAll(rdAssault, rdRaid, rdSlaughter);

			pnlAttackStrength.appendAll(lblAttackStrength, rdRaid, rdAssault, rdSlaughter);

			pnlButton.append(btnOk);

			append(pnlAttackStrength);
			append(pnlButton);
		}
	}

}