package src.UI.Tooltips {
    import org.aswing.*;

    import src.Global;
    import src.Map.Username;
    import src.Objects.Troop.TroopObject;
    import src.UI.LookAndFeel.GameLookAndFeel;

    public class TroopObjectTooltip extends Tooltip{

		private var lblCity: JLabel;

		public function TroopObjectTooltip(troopObj: TroopObject) {
			createUI();

			Global.map.usernames.cities.getUsername(troopObj.cityId, setCityName);
		}

		private function createUI() : void {
			lblCity = new JLabel(" ", null, AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(lblCity, "header");
		}
		
		private function setCityName(username: Username, custom: * = null): void
		{						
			lblCity.setText(username.name);
			ui.append(lblCity);
			resize();
		}
	}
}

