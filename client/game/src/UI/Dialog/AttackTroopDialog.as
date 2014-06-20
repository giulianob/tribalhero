package src.UI.Dialog {

    import flash.events.*;

    import org.aswing.*;
    import org.aswing.ext.*;
    import org.aswing.geom.*;

    import src.*;
    import src.Map.*;
    import src.Objects.Effects.*;
    import src.Objects.Troop.*;
    import src.UI.*;
    import src.UI.Components.*;
    import src.UI.Components.SimpleTroopGridList.*;
    import src.Util.*;

    public class AttackTroopDialog extends GameJPanel {

        public static const ATTACK_NONE: int = -1;
        public static const ATTACK_RAID: int = 0;
        public static const ATTACK_ASSAULT: int = 1;
        public static const ATTACK_SLAUGHTER: int = 2;

		//members define
		protected var pnlAttackStrength:JPanel;
		protected var lblAttackStrength:JLabel;
		protected var rdAssault:JRadioButton;
		protected var rdRaid:JRadioButton;
		protected var rdSlaughter:JRadioButton;
		protected var pnlLocal:JTabbedPane;
		protected var pnlAttack:JTabbedPane;
		protected var pnlButton:JPanel;
		protected var btnOk:JButton;
		protected var radioGroup: ButtonGroup;
		protected var lblTroopSpeed: MultilineLabel;

		protected var city: City;
		
		protected var hasAttackStrength: Boolean;

		protected var tilelists: Array = [];
		protected var attackTilelists: Array = [];
        private var defaultAttackMode: int;

		public function AttackTroopDialog(city: City, onAccept: Function, hasAttackStrength: Boolean = true, defaultAttackMode: int = ATTACK_RAID):void
		{
            title = "Send Attack";

			this.city = city;
			this.hasAttackStrength = hasAttackStrength;
            this.defaultAttackMode = defaultAttackMode;
			
			createUI();			

			var self: AttackTroopDialog = this;
			btnOk.addActionListener(function():void { 
				if (getTroop().getIndividualUnitCount() == 0) {
					InfoDialog.showMessageDialog("Error", "You have to assign units before continuing. Drag units from the local troop to assign them.");
					return;
				}

				if (hasAttackStrength && getMode() == ATTACK_NONE) {
					InfoDialog.showMessageDialog("Error", "You have to choose an attack strength before continuing. Select one above at the top.");
					return;
				}
				
				if (onAccept != null) onAccept(self); 
			} );

			//create local tile lists
			var localTilelists: Array = SimpleTroopGridList.getGridList(city.troops.getDefaultTroop(), city.template, [Formation.Normal]);

			pnlLocal.appendTab(SimpleTroopGridList.stackGridLists(localTilelists, false), "Local Troop");

			//create attack tile lists
			var newTroop: TroopStub = new TroopStub();
			newTroop.add(new Formation(Formation.Attack));

			attackTilelists = SimpleTroopGridList.getGridList(newTroop, city.template);

			pnlAttack.appendTab(SimpleTroopGridList.stackGridLists(attackTilelists, false), "Attack Troop");

			//drag handler
			tilelists = localTilelists.concat(attackTilelists);
			var tileListDragDropHandler: SimpleTroopGridDragHandler = new SimpleTroopGridDragHandler(tilelists);
			
			// troop speed label
			for each (var tilelist: SimpleTroopGridList in attackTilelists) {
				tilelist.addEventListener(SimpleTroopGridList.UNIT_CHANGED, updateSpeedInfo);
			}
			
			updateSpeedInfo();
		}
		
		protected function updateSpeedInfo(e: Event = null): void {
			var stub: TroopStub = getTroop();			
			if (stub.getIndividualUnitCount() == 0) {
				lblTroopSpeed.setText(StringHelper.localize("TROOP_CREATE_DRAG_HINT")) 
			}
			else {
				lblTroopSpeed.setText("Troop speed will be: " + Formula.moveTimeStringFull(stub.getSpeed(city)));
			}
		}

		public function getMode(): int
		{
			if (rdAssault.isSelected()) return ATTACK_ASSAULT;
			else if (rdRaid.isSelected()) return ATTACK_RAID;
			else if (rdSlaughter.isSelected()) return ATTACK_SLAUGHTER;
			else return ATTACK_NONE;
		}

		public function getTroop(): TroopStub
		{
			var newTroop: TroopStub = new TroopStub();
			newTroop.cityId = city.id;

			for (var i: int = 0; i < attackTilelists.length; i++)
			{
				newTroop.add((attackTilelists[i] as SimpleTroopGridList).getFormation());
			}

			return newTroop;
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
			layout0.setGap(10);
			setLayout(layout0);

			pnlAttackStrength = new JPanel();

			lblAttackStrength = new JLabel();
			lblAttackStrength.setText("Attack Strength:");

			rdAssault = new JRadioButton();
			rdAssault.setText("Assault");
            rdAssault.setSelected(defaultAttackMode == ATTACK_ASSAULT);
			new SimpleTooltip(rdAssault, "Retreat if only 1/3 of the units remain");

			rdRaid = new JRadioButton();
            rdRaid.setSelected(defaultAttackMode == ATTACK_RAID);
			rdRaid.setText("Raid");
			new SimpleTooltip(rdRaid, "Retreat if only 2/3 of the units remain");

			rdSlaughter = new JRadioButton();
            rdSlaughter.setSelected(defaultAttackMode == ATTACK_ASSAULT);
			rdSlaughter.setText("Slaughter");
			new SimpleTooltip(rdSlaughter, "Fight until death");

			pnlLocal = new JTabbedPane();
			pnlLocal.setSize(new IntDimension(389, 35));

			pnlAttack = new JTabbedPane();
			pnlAttack.setSize(new IntDimension(389, 35));

			pnlButton = new JPanel();
			var layout3:FlowLayout = new FlowLayout();
			layout3.setAlignment(AsWingConstants.CENTER);
			pnlButton.setLayout(layout3);

			btnOk = new JButton();
			btnOk.setLocation(new IntPoint(183, 5));
			btnOk.setSize(new IntDimension(22, 22));
			btnOk.setText("Ok");

			lblTroopSpeed = new MultilineLabel("", 2, 0);
			
			//component layoution
			if (hasAttackStrength) {
				append(pnlAttackStrength);
			}
			
			append(pnlLocal);
			append(pnlAttack);
			append(lblTroopSpeed);
			append(pnlButton);

			pnlAttackStrength.append(lblAttackStrength);

			radioGroup = new ButtonGroup();
			radioGroup.append(rdAssault);
			radioGroup.append(rdRaid);
			radioGroup.append(rdSlaughter);

			pnlAttackStrength.append(rdRaid);
			pnlAttackStrength.append(rdAssault);
			pnlAttackStrength.append(rdSlaughter);

			pnlButton.append(btnOk);
		}
	}

}

