package src.UI.Dialog {
	import fl.lang.Locale;
	import flash.display.*;
	import flash.events.*;
	import mx.utils.StringUtil;
	import org.aswing.*;
	import org.aswing.geom.*;
	import src.*;
	import src.Map.*;
	import src.Objects.*;
	import src.Objects.Troop.*;
	import src.UI.*;
	import src.UI.Components.*;
    import src.UI.Components.TroopCompositionGridList.TroopCompositionGridList;
    import src.UI.LookAndFeel.*;
	import src.UI.Tooltips.*;
    import src.Util.StringHelper;

    public class AssignmentInfoDialog extends GameJPanel {

		private var assignment: * ;
		private var tooltip: SimpleTroopStubTooltip;

		public function AssignmentInfoDialog(assignment: *):void
		{
			title = "Assignment Information";

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
			setPreferredSize(new IntDimension(350, 500));
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
			for each (var troop:* in assignment.troops) {
				var troopHolder: JPanel = new JPanel(new BorderLayout(5));

				var lblPlayerCity: PlayerCityLabel = new PlayerCityLabel(troop.playerId, troop.cityId, troop.playerName, troop.cityName);
				lblPlayerCity.setConstraints("Center");

				var lblUnits: JLabelButton = new SimpleTroopStubLabel("View Units", troop.stub);
				lblUnits.setConstraints("East");

				troopHolder.appendAll(lblPlayerCity, lblUnits);
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
	}

}

