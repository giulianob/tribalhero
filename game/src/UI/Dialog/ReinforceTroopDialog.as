package src.UI.Dialog {

	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.geom.*;
	import org.aswing.colorchooser.*;
	import org.aswing.ext.*;
	import src.Global;
	import src.Map.City;
	import src.UI.Components.SimpleTroopGridList.*;
	import src.UI.GameJPanel;
	import src.Objects.Troop.*;

	public class ReinforceTroopDialog extends GameJPanel {

		//members define
		private var pnlLocal:JPanel;
		private var pnlAttack:JPanel;
		private var panel9:JPanel;
		private var btnOk:JButton;
		private var radioGroup: ButtonGroup;

		private var city: City;

		private var tilelists: Array = new Array();
		private var attackTilelists: Array = new Array();
		private var destFormations: Array;

		public function ReinforceTroopDialog(city: City, srcTroop: TroopStub, destFormations: Array, onAccept: Function):void
		{
			createUI();

			title = "Send Reinforcement";

			this.city = city;
			this.destFormations = destFormations;

			var self: ReinforceTroopDialog = this;
			btnOk.addActionListener(function():void { if (onAccept != null) onAccept(self); } );

			//create local tile lists
			var localTilelists: Array = SimpleTroopGridList.getGridList(srcTroop, city.template, [Formation.Normal]);

			pnlLocal.append(SimpleTroopGridList.stackGridLists(localTilelists, false));

			//create attack tile lists
			var newTroop: TroopStub = new TroopStub();
			for (var i: int = 0; i < destFormations.length; i ++)
			newTroop.add(new Formation(destFormations[i]));

			attackTilelists = SimpleTroopGridList.getGridList(newTroop, city.template, destFormations);

			pnlAttack.append(SimpleTroopGridList.stackGridLists(attackTilelists, false));

			//drag handler
			tilelists = localTilelists.concat(attackTilelists);
			var tileListDragDropHandler: SimpleTroopGridDragHandler = new SimpleTroopGridDragHandler(tilelists);
		}

		public function getTroop(): TroopStub
		{
			var newTroop: TroopStub = new TroopStub();

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
			border2.setTitle("Reinforcement Troop");
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
			append(pnlLocal);
			append(pnlAttack);
			append(panel9);

			panel9.append(btnOk);
		}
	}

}

