package src.UI.Tooltips {
	import flash.events.Event;
	import flash.events.TimerEvent;
	import flash.utils.Timer;
	import org.aswing.JLabel;
	import org.aswing.JPanel;
	import org.aswing.SoftBoxLayout;
	import src.Global;
	import src.Map.Username;
	import src.Objects.Actions.StructureChangeAction;
	import src.Objects.Prototypes.StructurePrototype;
	import src.Objects.StructureObject;
	import src.Objects.Troop.TroopObject;
	import src.UI.LookAndFeel.GameLookAndFeel;

	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.geom.*;
	import org.aswing.colorchooser.*;
	import org.aswing.ext.*;

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

