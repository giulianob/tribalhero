package src.UI.Dialog {
	import src.Global;
	import src.Map.*;
	import src.Objects.*;
	import src.UI.Components.SimpleTroopGridList.SimpleTroopGridDragHandler;
	import src.UI.Components.SimpleTroopGridList.SimpleTroopGridList;
	import src.UI.GameJPanel;
	import src.Objects.Troop.*;

	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.geom.*;
	import org.aswing.colorchooser.*;
	import org.aswing.ext.*;

	public class UnitMoveDialog extends GameJPanel {

		private var pnlFormations:JPanel;
		private var pnlBottom:JPanel;
		private var btnOk:JButton;

		private var city: City;
		private var tilelists: Array = new Array();

		public function UnitMoveDialog(city: City, onAccept: Function)
		{
			createUI();
			title = "Assign Units";

			var self: UnitMoveDialog = this;
			btnOk.addActionListener(function():void { if (onAccept != null) onAccept(self); } );

			this.city = city;

			tilelists = new Array();

			var troop: TroopStub = city.troops.getDefaultTroop();

			drawTroop(troop, [Formation.Normal, Formation.Garrison]);
		}

		public function drawTroop(troop: TroopStub, formations: Array = null):void
		{
			tilelists = SimpleTroopGridList.getGridList(troop, city.template, formations);

			pnlFormations.append(SimpleTroopGridList.stackGridLists(tilelists));

			var tileListDragDropHandler: SimpleTroopGridDragHandler = new SimpleTroopGridDragHandler(tilelists);
		}

		public function getTroop(): TroopStub
		{
			var newTroop: TroopStub = new TroopStub();

			for (var i: int = 0; i < tilelists.length; i++)
			{
				newTroop.add((tilelists[i] as SimpleTroopGridList).getFormation());
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

			pnlFormations = new JPanel();
			pnlFormations.setSize(new IntDimension(400, 10));

			pnlBottom = new JPanel();
			pnlBottom.setLocation(new IntPoint(5, 5));
			pnlBottom.setSize(new IntDimension(10, 10));
			var layout1:FlowLayout = new FlowLayout();
			layout1.setAlignment(AsWingConstants.CENTER);
			pnlBottom.setLayout(layout1);

			btnOk = new JButton();
			btnOk.setLocation(new IntPoint(184, 5));
			btnOk.setSize(new IntDimension(31, 22));
			btnOk.setText("Ok");

			//component layoution
			append(pnlFormations);
			append(pnlBottom);

			pnlBottom.append(btnOk);

		}
	}

}

