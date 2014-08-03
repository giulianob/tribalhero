package src.UI.Dialog {

    import flash.events.Event;

    import org.aswing.*;
    import org.aswing.geom.*;

    import src.Global;
    import src.Map.City;
    import src.Objects.Effects.Formula;
    import src.Objects.Troop.*;
    import src.UI.Components.SimpleTooltip;
    import src.UI.Components.SimpleTroopGridList.*;
    import src.UI.GameJPanel;
    import src.Util.StringHelper;

    public class ReinforceTroopDialog extends GameJPanel {
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
		protected var panel9:JPanel;
		protected var btnOk:JButton;
		protected var radioGroup: ButtonGroup;		
		protected var lblTroopSpeed: JLabel;

		protected var city: City;

		protected var hasAttackStrength: Boolean;
		
		private var tilelists: Array = [];
		private var attackTilelists: Array = [];
        private var defaultAttackMode: int;

		public function ReinforceTroopDialog(city: City, onAccept: Function, hasAttackStrength: Boolean = true, defaultAttackMode: int = ATTACK_RAID): void
		{
			title = "Send Reinforcement";

            this.defaultAttackMode = defaultAttackMode;
			this.city = city;
			this.hasAttackStrength = hasAttackStrength;

			createUI();

			var self: ReinforceTroopDialog = this;
			btnOk.addActionListener(function():void { 
				if (getTroop().getIndividualUnitCount() == 0) {
					InfoDialog.showMessageDialog("Error", "You have to assign units before continuing. Drag units from the local troop to assign them.");
					return;
				}

                if (hasAttackStrength && getMode() == ATTACK_NONE) {
                    InfoDialog.showMessageDialog("Error", "You have to choose a defense strength before continuing. Select one above at the top.");
                    return;
                }

				if (onAccept != null) onAccept(self); 
			} );

			//create local tile lists
			var localTilelists: Array = SimpleTroopGridList.getGridList(city.troops.getDefaultTroop(), city.template, [Formation.Normal]);

			pnlLocal.appendTab(SimpleTroopGridList.stackGridLists(localTilelists, false), "Local Troop");

			//create attack tile lists
			var newTroop: TroopStub = new TroopStub();
			newTroop.add(new Formation(Formation.Defense));

			attackTilelists = SimpleTroopGridList.getGridList(newTroop, city.template, [Formation.Defense]);

			pnlAttack.appendTab(SimpleTroopGridList.stackGridLists(attackTilelists, false), "Reinforcement Troop");

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
				lblTroopSpeed.setText(StringHelper.localize("TROOP_CREATE_DRAG_HINT"));
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
			lblAttackStrength.setText("Defense Strength:");

			rdAssault = new JRadioButton();
            rdAssault.setSelected(defaultAttackMode == ATTACK_ASSAULT);
			rdAssault.setText("Assault");
			new SimpleTooltip(rdAssault, "Retreat if only 1/3 of the units remain");

			rdRaid = new JRadioButton();
            rdRaid.setSelected(defaultAttackMode == ATTACK_RAID);
			rdRaid.setText("Raid");
			new SimpleTooltip(rdRaid, "Retreat if only 2/3 of the units remain");

			rdSlaughter = new JRadioButton();
            rdSlaughter.setSelected(defaultAttackMode == ATTACK_SLAUGHTER);
			rdSlaughter.setText("Slaughter");
			new SimpleTooltip(rdSlaughter, "Fight until death");
			
			pnlLocal = new JTabbedPane();
			pnlLocal.setSize(new IntDimension(389, 35));

			pnlAttack = new JTabbedPane();
			pnlAttack.setSize(new IntDimension(389, 35));

			panel9 = new JPanel();
			var layout3:FlowLayout = new FlowLayout();
			layout3.setAlignment(AsWingConstants.CENTER);
			panel9.setLayout(layout3);

			btnOk = new JButton();
			btnOk.setText("Ok");
			
			lblTroopSpeed = new JLabel("", null, AsWingConstants.LEFT);

			//component layoution
			if (hasAttackStrength) {
				append(pnlAttackStrength);
			} else {
				append(new JLabel("Hint: Drag units from local troop to assign for reinforcement", null, AsWingConstants.LEFT));
			}
			
			append(pnlLocal);
			append(pnlAttack);
			append(lblTroopSpeed);
			append(panel9);

			pnlAttackStrength.append(lblAttackStrength);

			radioGroup = new ButtonGroup();
			radioGroup.append(rdAssault);
			radioGroup.append(rdRaid);
			radioGroup.append(rdSlaughter);

			pnlAttackStrength.append(rdRaid);
			pnlAttackStrength.append(rdAssault);
			pnlAttackStrength.append(rdSlaughter);
			
			panel9.append(btnOk);
		}
	}

}

