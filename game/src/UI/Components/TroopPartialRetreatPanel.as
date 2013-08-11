package src.UI.Components {
    import flash.events.Event;

    import org.aswing.AsWingConstants;
    import org.aswing.ButtonGroup;
    import org.aswing.JPanel;
    import org.aswing.JRadioButton;
    import org.aswing.JTabbedPane;
    import org.aswing.SoftBoxLayout;
    import org.aswing.event.InteractiveEvent;
    import org.aswing.event.SelectionEvent;
    import org.aswing.geom.IntDimension;

    import src.Global;
    import src.Map.City;
    import src.Objects.Troop.Formation;
    import src.Objects.Troop.TroopStub;
    import src.UI.Components.SimpleTroopGridList.SimpleTroopGridDragHandler;
    import src.UI.Components.SimpleTroopGridList.SimpleTroopGridList;
    import src.Util.StringHelper;

    public class TroopPartialRetreatPanel extends JPanel {

        protected var radioGroup: ButtonGroup;
        protected var rdRetreatAll:JRadioButton;
        protected var rdRetreatPartial:JRadioButton;
        protected var pnlStationed:JTabbedPane;
        protected var pnlUnitsToRetreat:JTabbedPane;

        protected var tilelists: Array = [];
        protected var retreatTilelists: Array = [];

        protected var troop: TroopStub;
        protected var city: City;
        private var localizationPrefix: String;

        public function TroopPartialRetreatPanel(troop: TroopStub, localizationPrefix: String) {
            this.troop = troop;
            this.localizationPrefix = localizationPrefix;

            this.city = Global.map.cities.get(troop.cityId);

            createUI();

            //create local tile lists
            var localTilelists: Array = SimpleTroopGridList.getGridList(troop, city.template, [Formation.Defense]);

            pnlStationed.appendTab(SimpleTroopGridList.stackGridLists(localTilelists, false),
                    StringHelper.localize(this.localizationPrefix + "_PARTIAL_DIALOG_SOURCE_TAB"));

            //create retreat tile lists
            var newTroop: TroopStub = new TroopStub();
            newTroop.add(new Formation(Formation.Defense));

            retreatTilelists = SimpleTroopGridList.getGridList(newTroop, city.template);

            pnlUnitsToRetreat.appendTab(SimpleTroopGridList.stackGridLists(retreatTilelists, false),
                    StringHelper.localize(this.localizationPrefix + "_PARTIAL_DIALOG_TARGET_TAB"));

            //drag handler
            tilelists = localTilelists.concat(retreatTilelists);
            var tileListDragDropHandler: SimpleTroopGridDragHandler = new SimpleTroopGridDragHandler(tilelists);

            var self: TroopPartialRetreatPanel = this;
            rdRetreatPartial.addSelectionListener(function(e: Event): void {
                pnlStationed.setVisible(rdRetreatPartial.isSelected());
                pnlUnitsToRetreat.setVisible(rdRetreatPartial.isSelected());

                self.dispatchEvent(new InteractiveEvent(InteractiveEvent.SELECTION_CHANGED));
            });
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

        public function shouldRetreatAll(): Boolean
        {
            return rdRetreatAll.isSelected();
        }

        private function createUI(): void {
            setLayout(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 10));

            var pnlRetreatQuestion: JPanel = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS));

            rdRetreatAll = new JRadioButton();
            rdRetreatAll.setHorizontalAlignment(AsWingConstants.LEFT);
            rdRetreatAll.setText(StringHelper.localize(this.localizationPrefix + "_PARTIAL_DIALOG_SELECT_ALL"));
            rdRetreatAll.setSelected(true);

            rdRetreatPartial = new JRadioButton();
            rdRetreatPartial.setHorizontalAlignment(AsWingConstants.LEFT);
            rdRetreatPartial.setText(StringHelper.localize(this.localizationPrefix + "_PARTIAL_DIALOG_SELECT_PARTIAL"));

            pnlStationed = new JTabbedPane();
            pnlStationed.setSize(new IntDimension(389, 35));
            pnlStationed.setVisible(false);

            pnlUnitsToRetreat = new JTabbedPane();
            pnlUnitsToRetreat.setSize(new IntDimension(389, 35));
            pnlUnitsToRetreat.setVisible(false);

            append(pnlRetreatQuestion);
            append(pnlStationed);
            append(pnlUnitsToRetreat);

            radioGroup = new ButtonGroup();
            radioGroup.appendAll(rdRetreatAll, rdRetreatPartial);

            pnlRetreatQuestion.appendAll(rdRetreatAll, rdRetreatPartial);
        }
    }
}
