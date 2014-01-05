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

		public function ReinforceTroopDialog(city: City, onAccept: Function, hasAttackStrength: Boolean = true):void
		{
			title = "Send Reinforcement";

			this.city = city;
			this.hasAttackStrength = hasAttackStrength;

			createUI();

			var self: ReinforceTroopDialog = this;
			btnOk.addActionListener(function():void { 
				if (getTroop().getIndividualUnitCount() == 0) {
					InfoDialog.showMessageDialog("Error", "You have to assign units before continuing. Drag units from the local troop to assign them.");
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
			if (rdAssault.isSelected()) return 1;
			else if (rdRaid.isSelected()) return 0;
			else if (rdSlaughter.isSelected()) return 2;
			else return -1;
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
			pnlAttackStrength.setLocation(new IntPoint(5, 5));
			pnlAttackStrength.setSize(new IntDimension(10, 10));

			lblAttackStrength = new JLabel();
			lblAttackStrength.setLocation(new IntPoint(5, 5));
			lblAttackStrength.setSize(new IntDimension(80, 17));
			lblAttackStrength.setText("Defense Strength:");

			rdAssault = new JRadioButton();
			rdAssault.setSelected(true);
			rdAssault.setLocation(new IntPoint(5, 5));
			rdAssault.setSize(new IntDimension(54, 17));
			rdAssault.setText("Assault");
			new SimpleTooltip(rdAssault, "Retreat if only 1/3 of the units remain");

			rdRaid = new JRadioButton();
			rdRaid.setLocation(new IntPoint(51, 5));
			rdRaid.setSize(new IntDimension(40, 17));
			rdRaid.setText("Raid");
			new SimpleTooltip(rdRaid, "Retreat if only 2/3 of the units remain");

			rdSlaughter = new JRadioButton();
			rdSlaughter.setLocation(new IntPoint(97, 5));
			rdSlaughter.setSize(new IntDimension(65, 17));
			rdSlaughter.setText("Slaughter");
			new SimpleTooltip(rdSlaughter, "Fight until death");
			
			pnlLocal = new JTabbedPane();
			pnlLocal.setSize(new IntDimension(389, 35));

			pnlAttack = new JTabbedPane();
			pnlAttack.setSize(new IntDimension(389, 35));

			panel9 = new JPanel();
			panel9.setLocation(new IntPoint(0, 127));
			panel9.setSize(new IntDimension(389, 10));
			var layout3:FlowLayout = new FlowLayout();
			layout3.setAlignment(AsWingConstants.CENTER);
			panel9.setLayout(layout3);

			btnOk = new JButton();
			btnOk.setLocation(new IntPoint(183, 5));
			btnOk.setSize(new IntDimension(22, 22));
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

