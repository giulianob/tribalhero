package src.UI.Tooltips {
	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.colorchooser.*;
	import org.aswing.ext.*;
	import org.aswing.geom.*;
	import src.*;
	import src.Map.*;
	import src.Objects.Troop.*;
	import src.UI.Components.ComplexTroopGridList.*;
	import src.UI.LookAndFeel.*;


	public class SimpleTroopStubTooltip extends Tooltip {
		private var pnlHeader: JPanel;
		private var pnlTop: JPanel;
		private var lblName: JLabel;
		private var troop: TroopStub;

		public function SimpleTroopStubTooltip(troop: TroopStub) {
			this.troop = troop;

			createUI();

			Global.map.usernames.cities.getUsername(troop.cityId, onReceiveCityUsername);

			if (troop.getIndividualUnitCount() == 0) {
				var lblQuietHere: JLabel = new JLabel("It's quiet in here", null, AsWingConstants.LEFT);
				GameLookAndFeel.changeClass(lblQuietHere, "Tooltip.text");
				ui.append(lblQuietHere);
			}
			else {
				var tilelists: Array = ComplexTroopGridList.getGridList(troop, true);
				ui.append(ComplexTroopGridList.stackGridLists(tilelists, true));
			}

			ui.pack();
		}

		private function onReceiveCityUsername(username: Username, custom: *) : void {
			lblName.setText(username.name + troop.getNiceId(true));
		}

		private function createUI() : void {
			ui.setLayout(new SoftBoxLayout(AsWingConstants.VERTICAL, 5));
			ui.setMinimumWidth(250);

			pnlTop = new JPanel(new BorderLayout(20));

			lblName = new JLabel();
			lblName.setHorizontalAlignment(AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(lblName, "header");
			lblName.setConstraints("West");

			pnlHeader = new JPanel(new SoftBoxLayout(AsWingConstants.VERTICAL, 0));
			pnlHeader.setBorder(null);

			pnlTop.append(lblName);
			
			ui.append(pnlTop);
		}

	}
}

