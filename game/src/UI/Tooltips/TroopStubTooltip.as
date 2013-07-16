package src.UI.Tooltips {
    import org.aswing.*;

    import src.Global;
    import src.Map.City;
    import src.Map.Username;
    import src.Objects.Troop.*;
    import src.UI.Components.ComplexTroopGridList.ComplexTroopGridList;
    import src.UI.Components.SimpleResourcesPanel;
    import src.UI.LookAndFeel.GameLookAndFeel;

    public class TroopStubTooltip extends Tooltip {
		private var lblName: JLabel;
		private var troop: TroopStub;
		private var city: City;
		private var pnlTop: JPanel;

		public function TroopStubTooltip(troop: TroopStub) {
			this.troop = troop;
			this.city = city;

			createUI();

			Global.map.usernames.cities.getUsername(troop.cityId, onReceiveCityUsername);				
		}

		private function onReceiveCityUsername(username: Username, custom: *) : void {
			lblName.setText(username.name + troop.getNiceId(true));
		}

		private function createUI() : void {
			ui.setLayout(new SoftBoxLayout(AsWingConstants.VERTICAL, 5));
			ui.setMinimumWidth(150);

			pnlTop = new JPanel(new BorderLayout(10));
			
			if (troop.resources != null && troop.resources.total() > 0) {
				var resource:SimpleResourcesPanel = new SimpleResourcesPanel(troop.resources, false, true);
				resource.setConstraints("East");
				pnlTop.append(resource);
			}			
			
			lblName = new JLabel();
			lblName.setHorizontalAlignment(AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(lblName, "header");
			lblName.setConstraints("Center");
			pnlTop.append(lblName);
			
			ui.append(pnlTop);
			
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

	}
}

