package src.UI.Dialog {

    import flash.events.Event;

    import org.aswing.*;
    import org.aswing.geom.*;

    import src.*;
    import src.Map.*;
    import src.Objects.Troop.*;
    import src.UI.*;
    import src.UI.Components.SimpleTroopGridList.*;
    import src.Util.Util;

    public class RetreatTroopDialog extends GameJPanel {

		//members define
		protected var rdRetreatAll:JRadioButton;
        protected var rdRetreatPartial:JRadioButton;
		protected var pnlLocal:JTabbedPane;
		protected var pnlRetreat:JTabbedPane;
		protected var pnlButton:JPanel;
		protected var btnOk:JButton;
		protected var radioGroup: ButtonGroup;

		protected var city: City;
        protected var troop: TroopStub;

		protected var tilelists: Array = new Array();
		protected var retreatTilelists: Array = new Array();

		public function RetreatTroopDialog(troop: TroopStub, onAccept: Function):void
		{
			title = "Retreat";

			this.city = Global.map.cities.get(troop.cityId);
            this.troop = troop;

			createUI();			

			var self: RetreatTroopDialog = this;
			btnOk.addActionListener(function():void { 
				if (getTroop().getIndividualUnitCount() == 0) {
					InfoDialog.showMessageDialog("Error", "You have to choose which units to retreat. Drag the units from the troop to retreat them.");
					return;
				}
				
				if (onAccept != null) onAccept(self); 
			} );

			//create local tile lists
			var localTilelists: Array = SimpleTroopGridList.getGridList(troop, city.template, [Formation.Defense]);

			pnlLocal.appendTab(SimpleTroopGridList.stackGridLists(localTilelists, false), "Troop");

			//create retreat tile lists
			var newTroop: TroopStub = new TroopStub();
			newTroop.add(new Formation(Formation.Defense));

			retreatTilelists = SimpleTroopGridList.getGridList(newTroop, city.template);

			pnlRetreat.appendTab(SimpleTroopGridList.stackGridLists(retreatTilelists, false), "Units To Retreat");

			//drag handler
			tilelists = localTilelists.concat(retreatTilelists);
			var tileListDragDropHandler: SimpleTroopGridDragHandler = new SimpleTroopGridDragHandler(tilelists);

            rdRetreatAll.addSelectionListener(function(e: Event): void {
                pnlLocal.setVisible(!rdRetreatAll.isSelected());
                pnlRetreat.setVisible(!rdRetreatAll.isSelected());

                getFrame().pack();
                Util.centerFrame(getFrame());
            });
		}

        public function shouldRetreatAll(): Boolean
        {
            return rdRetreatAll.isSelected();
        }

		public function getTroop(): TroopStub
		{
            if (rdRetreatAll.isSelected()) {
                return troop;
            }

			var newTroop: TroopStub = new TroopStub();
			newTroop.cityId = city.id;

			for (var i: int = 0; i < retreatTilelists.length; i++)
			{
				newTroop.add((retreatTilelists[i] as SimpleTroopGridList).getFormation());
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
			setLayout(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 10));
            setPreferredWidth(425);

			var pnlRetreatQuestion: JPanel = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS));

			rdRetreatAll = new JRadioButton();
            rdRetreatAll.setSelected(true);
            rdRetreatAll.setHorizontalAlignment(AsWingConstants.LEFT);
            rdRetreatAll.setText("Retreat entire troop");

            rdRetreatPartial = new JRadioButton();
            rdRetreatPartial.setHorizontalAlignment(AsWingConstants.LEFT);
            rdRetreatPartial.setText("Let me choose which units to retreat");

			pnlLocal = new JTabbedPane();
			pnlLocal.setSize(new IntDimension(389, 35));
            pnlLocal.setVisible(false);

			pnlRetreat = new JTabbedPane();
            pnlRetreat.setSize(new IntDimension(389, 35));
            pnlRetreat.setVisible(false);

			pnlButton = new JPanel();
			var layout3:FlowLayout = new FlowLayout();
			layout3.setAlignment(AsWingConstants.CENTER);
			pnlButton.setLayout(layout3);

			btnOk = new JButton();
			btnOk.setText("Ok");

            append(pnlRetreatQuestion);
			append(pnlLocal);
			append(pnlRetreat);
			append(pnlButton);

			radioGroup = new ButtonGroup();
			radioGroup.appendAll(rdRetreatAll, rdRetreatPartial);

			pnlRetreatQuestion.appendAll(rdRetreatAll, rdRetreatPartial);

			pnlButton.append(btnOk);
		}
	}

}

