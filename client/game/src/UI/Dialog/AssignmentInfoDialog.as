package src.UI.Dialog {
    import System.Linq.Enumerable;

    import fl.lang.Locale;

    import flash.events.Event;

    import mx.utils.StringUtil;

    import org.aswing.*;
    import org.aswing.geom.*;

    import src.*;
    import src.Objects.Troop.*;
    import src.UI.*;
    import src.UI.Components.*;
    import src.UI.Components.TroopCompositionGridList.TroopCompositionGridList;
    import src.UI.LookAndFeel.*;
    import src.Util.FunctionUtil;
    import src.Util.StringHelper;

    public class AssignmentInfoDialog extends GameJPanel {

		public var assignment: * ;
        private var onChange: Function;

		public function AssignmentInfoDialog(assignment: *, onChange: Function):void
		{
			title = "Assignment Information";

            this.onChange = onChange;
			this.assignment = assignment;

			createUI();
		}

		public function show(owner:* = null, modal:Boolean = true, onClose:Function = null):JFrame
		{
			super.showSelf(owner, modal, onClose);
			Global.gameContainer.showFrame(frame);
			return frame;
		}

		private function createRow(...coms): JPanel {
			var holder: JPanel = new JPanel(new SoftBoxLayout());

			for each(var i:* in coms) {
				holder.append(i);
			}
			return holder;
		}

		private function createUI(): void {
            removeAll();

			setPreferredSize(new IntDimension(450, 500));
			setLayout(new BorderLayout(5));

			var pnlHeader: JPanel = new JPanel(new BorderLayout(5, 5));
			pnlHeader.setConstraints("North");

			var lblCountdown: CountDownLabel = new CountDownLabel(assignment.endTime);
			lblCountdown.setVerticalAlignment(AsWingConstants.TOP);
			lblCountdown.setConstraints("East");
			var pnlDetails: JPanel = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS));
			pnlDetails.setConstraints("Center");

			if(assignment.troops.length>0)
				pnlDetails.append(createRow(new JLabel("Organized by"), new PlayerLabel(assignment.troops[0].playerId, assignment.troops[0].playerName)));
			pnlDetails.append(createRow(new RichLabel(StringUtil.substitute(Locale.loadString(assignment.isAttack?"ASSIGNMENT_ATK":"ASSIGNMENT_DEF"), RichLabel.getHtmlForLocation(assignment.target)), 1)));
			pnlDetails.append(createRow(new JLabel("Target"), new CoordLabel(assignment.x, assignment.y)));
			if (assignment.attackMode == 0)
				pnlDetails.append(createRow(new JLabel("Strength:"), new JLabel("Raid")));
			else if (assignment.attackMode == 1)
				pnlDetails.append(createRow(new JLabel("Strength:"), new JLabel("Assault")));
			else if ( assignment.attackMode == 2)
				pnlDetails.append(createRow(new JLabel("Strength:"), new JLabel("Slaughter")));

            var gridList: TroopCompositionGridList  = new TroopCompositionGridList(null, 3, 0);
            var lblTotal: JLabel = new JLabel(StringHelper.localize("STR_TOTAL_TROOPS"));
            lblTotal.setVerticalAlignment(AsWingConstants.TOP);
            pnlDetails.append((createRow(lblTotal,gridList)));

            var lblTroops: JLabel = new JLabel("Troops", null, AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(lblTroops, "darkHeader");
			pnlDetails.appendAll(new JLabel(" "), lblTroops);

            var total: TroopStub = new TroopStub();
			var pnlTroops: JPanel = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 10));
			for each (var troop: * in assignment.troops) {
				var troopHolder: JPanel = new JPanel(new BorderLayout(5));

				var lblPlayerCity: PlayerCityLabel = new PlayerCityLabel(troop.playerId, troop.cityId, troop.playerName, troop.cityName);
				lblPlayerCity.setConstraints("Center");

                var actionsHolder: JPanel = new JPanel(new FlowLayout(AsWingConstants.RIGHT, 10, 10, false));
                actionsHolder.setConstraints("East");

                if (Global.map.cities.get(troop.cityId)) {
                    var lblRemove: JLabelButton = new JLabelButton("Remove");
                    var cityId: int = troop.cityId;
                    var stubId: int = troop.stub.id;
                    var assignmentId: int = assignment.id;
                    lblRemove.addActionListener(FunctionUtil.bind(removeStub, this, assignmentId, cityId, stubId));

                    actionsHolder.append(lblRemove);
                }

				var lblUnits: JLabelButton = new SimpleTroopStubLabel("View Units", troop.stub);

                actionsHolder.appendAll(lblUnits);
				troopHolder.appendAll(lblPlayerCity, actionsHolder);
				pnlTroops.append(troopHolder);
                total.addTroop(troop.stub);
			}
            gridList.setTroop(total);

            var scrollTroops: JScrollPane = new JScrollPane(new JViewport(pnlTroops, true), JScrollPane.SCROLLBAR_ALWAYS, JScrollPane.SCROLLBAR_NEVER);
			(scrollTroops.getViewport() as JViewport).setVerticalAlignment(AsWingConstants.TOP);
			scrollTroops.setConstraints("Center");

			pnlHeader.appendAll(pnlDetails, lblCountdown);
			appendAll(pnlHeader, scrollTroops);
		}

        private function removeStub(assignmentId: int, cityId: int, stubId: int, e: Event): void {
            InfoDialog.showMessageDialog("Confirm", "Are you sure you want to remove the troop from this assignment?", function(result: int): void {
                if (result != JOptionPane.YES) {
                    return;
                }

                Global.mapComm.Tribe.removeFromAssignment(assignmentId, cityId, stubId).then(function(): void {
                    if (onChange != null) {
                        onChange();
                    }
                })

            }, null, true, true, JOptionPane.YES | JOptionPane.NO);
        }
	}

}

