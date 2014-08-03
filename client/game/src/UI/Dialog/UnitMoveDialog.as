package src.UI.Dialog {
    import org.aswing.*;
    import org.aswing.geom.*;

    import src.Global;
    import src.Map.*;
    import src.Objects.Troop.*;
    import src.UI.Components.SimpleTooltip;
    import src.UI.Components.SimpleTroopGridList.SimpleTroopGridDragHandler;
    import src.UI.Components.SimpleTroopGridList.SimpleTroopGridList;
    import src.UI.GameJPanel;
    import src.Util.StringHelper;

    public class UnitMoveDialog extends GameJPanel {

		private var pnlFormations:JPanel;
		private var pnlBottom:JPanel;
		private var btnOk:JButton;
		private var pnlNewUnits:JPanel;
		private var chkHideNewUnits:JCheckBox;

		private var city: City;
		private var tilelists: Array = [];

		public function UnitMoveDialog(city: City, onAccept: Function)
		{
			createUI();
			title = StringHelper.localize("UNIT_MANAGE_DIALOG_TITLE");

			var self: UnitMoveDialog = this;
			btnOk.addActionListener(function():void { if (onAccept != null) onAccept(self); } );

			this.city = city;

			chkHideNewUnits.setSelected(city.hideNewUnits);

			tilelists = [];

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
		
		public function getHideNewUnits(): Boolean 
		{
			return chkHideNewUnits.isSelected();
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

			chkHideNewUnits = new JCheckBox(StringHelper.localize("UNIT_MANAGE_HIDE_NEW_UNITS"));
			new SimpleTooltip(chkHideNewUnits, StringHelper.localize("UNIT_MANAGE_HIDE_NEW_UNITS_TOOLTIP"));

			pnlNewUnits = new JPanel(new FlowLayout(AsWingConstants.LEFT, 5));
			pnlNewUnits.append(chkHideNewUnits);

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
			btnOk.setText(StringHelper.localize("STR_SAVE"));

			//component layoution
			append(pnlNewUnits);
			append(pnlFormations);
			append(pnlBottom);
            
            append(new JLabel(StringHelper.localize("UNIT_MANAGE_DIALOG_TIP"), null, AsWingConstants.LEFT));

			pnlBottom.append(btnOk);

		}
	}

}

