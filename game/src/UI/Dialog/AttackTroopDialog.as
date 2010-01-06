package src.UI.Dialog {

	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.geom.*;
	import org.aswing.colorchooser.*;
	import org.aswing.ext.*;
	import src.Global;
	import src.Map.City;
	import src.UI.Components.SimpleTooltip;
	import src.UI.Components.SimpleTroopGridList.*;
	import src.UI.GameJPanel;
	import src.Objects.Troop.*;

	public class AttackTroopDialog extends GameJPanel {

		//members define
		private var panel2:JPanel;
		private var lblAttackStrength:JLabel;
		private var rdAssault:JRadioButton;
		private var rdRaid:JRadioButton;
		private var rdSlaughter:JRadioButton;
		private var pnlLocal:JPanel;
		private var pnlAttack:JPanel;
		private var panel9:JPanel;
		private var btnOk:JButton;
		private var radioGroup: ButtonGroup;

		private var city: City;

		private var tilelists: Array = new Array();
		private var attackTilelists: Array = new Array();
		private var destFormations: Array;

		public function AttackTroopDialog(city: City, srcTroop: Troop, destFormations: Array, onAccept: Function):void
		{
			createUI();

			title = "Send Attack";

			this.city = city;
			this.destFormations = destFormations;

			var self: AttackTroopDialog = this;
			btnOk.addActionListener(function():void { if (onAccept != null) onAccept(self); } );

			//create local tile lists
			var localTilelists: Array = SimpleTroopGridList.getGridList(srcTroop, city.template, [Formation.Normal]);

			pnlLocal.append(SimpleTroopGridList.stackGridLists(localTilelists));

			//create attack tile lists
			var newTroop: Troop = new Troop();
			for (var i: int = 0; i < destFormations.length; i ++)
			newTroop.add(new Formation(destFormations[i]));

			attackTilelists = SimpleTroopGridList.getGridList(newTroop, city.template);

			pnlAttack.append(SimpleTroopGridList.stackGridLists(attackTilelists));

			//drag handler
			tilelists = localTilelists.concat(attackTilelists);
			var tileListDragDropHandler: SimpleTroopGridDragHandler = new SimpleTroopGridDragHandler(tilelists);
		}

		public function getMode(): int
		{
			if (rdAssault.isSelected())
			return 1;
			else if (rdRaid.isSelected())
			return 0;
			else if (rdSlaughter.isSelected())
			return 2;
			else
			return -1;
		}

		public function getTroop(): Troop
		{
			var newTroop: Troop = new Troop();

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

			panel2 = new JPanel();
			panel2.setLocation(new IntPoint(5, 5));
			panel2.setSize(new IntDimension(10, 10));

			lblAttackStrength = new JLabel();
			lblAttackStrength.setLocation(new IntPoint(5, 5));
			lblAttackStrength.setSize(new IntDimension(80, 17));
			lblAttackStrength.setText("Attack Strength:");

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

			pnlLocal = new JPanel();
			pnlLocal.setLocation(new IntPoint(278, 13));
			pnlLocal.setSize(new IntDimension(389, 35));
			var border1:TitledBorder = new TitledBorder();
			border1.setColor(new ASColor(0x0, 1));
			border1.setTitle("Local Troop");
			border1.setPosition(1);
			border1.setBeveled(true);
			border1.setEdge(0);
			border1.setRound(5);
			pnlLocal.setBorder(border1);

			pnlAttack = new JPanel();
			pnlAttack.setLocation(new IntPoint(0, 82));
			pnlAttack.setSize(new IntDimension(389, 35));
			var border2:TitledBorder = new TitledBorder();
			border2.setColor(new ASColor(0x0, 1));
			border2.setTitle("Attack Troop");
			border2.setPosition(1);
			border2.setBeveled(true);
			border2.setRound(5);
			pnlAttack.setBorder(border2);

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

			//component layoution
			append(panel2);
			append(pnlLocal);
			append(pnlAttack);
			append(panel9);

			panel2.append(lblAttackStrength);

			radioGroup = new ButtonGroup();
			radioGroup.append(rdAssault);
			radioGroup.append(rdRaid);
			radioGroup.append(rdSlaughter);

			panel2.append(rdRaid);
			panel2.append(rdAssault);			
			panel2.append(rdSlaughter);

			panel9.append(btnOk);
		}
	}

}

