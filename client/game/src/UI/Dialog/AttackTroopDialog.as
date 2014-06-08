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

		public function AttackTroopDialog(city: City, onAccept: Function, hasAttackStrength: Boolean = true):void
		{
			title = "Send Attack";

			this.city = city;
			this.hasAttackStrength = hasAttackStrength;
			
			createUI();			

			var self: AttackTroopDialog = this;
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
			lblAttackStrength.setText("Attack Strength:");

			rdAssault = new JRadioButton();
			rdAssault.setLocation(new IntPoint(5, 5));
			rdAssault.setSize(new IntDimension(54, 17));
			rdAssault.setText("Assault");
			new SimpleTooltip(rdAssault, "Retreat if only 1/3 of the units remain");

			rdRaid = new JRadioButton();
            rdRaid.setSelected(true);
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

			pnlButton = new JPanel();
			pnlButton.setLocation(new IntPoint(0, 127));
			pnlButton.setSize(new IntDimension(389, 10));
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

