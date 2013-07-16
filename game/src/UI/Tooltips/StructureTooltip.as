package src.UI.Tooltips {
    import org.aswing.*;

    import src.Global;
    import src.Map.Username;
    import src.Objects.Prototypes.StructurePrototype;
    import src.Objects.StructureObject;
    import src.UI.LookAndFeel.GameLookAndFeel;

    public class StructureTooltip extends Tooltip{

		private var structurePrototype: StructurePrototype;
	
		private var lblName: JLabel;
		private var lblLevel: JLabel;
		private var lblCity: JLabel;

		// If structObj is passed then city and player name are shown. Bit dirty.
		public function StructureTooltip(structObj: StructureObject, structurePrototype: StructurePrototype) {
			this.structurePrototype = structurePrototype;

			createUI();

			lblName.setText(structurePrototype.getName());
			lblLevel.setText("Level " + structurePrototype.level);
			
			if (structObj) {
				Global.map.usernames.cities.getUsername(structObj.cityId, setCityName);
			}
		}

		private function createUI() : void {
			ui.setLayout(new BorderLayout(20, 0));

			lblName = new JLabel();
			lblName.setHorizontalAlignment(AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(lblName, "Tooltip.text");
			lblName.setConstraints("Center");

			lblLevel = new JLabel();
			lblLevel.setHorizontalAlignment(AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(lblLevel, "Tooltip.text");
			lblLevel.setConstraints("East");
			
			lblCity = new JLabel(" ", null, AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(lblCity, "header");
			lblCity.setConstraints("North");			

			ui.append(lblName);
			ui.append(lblLevel);			
		}
		
		private function setCityName(username: Username, custom: * = null): void
		{						
			lblCity.setText(username.name);
			ui.append(lblCity);
			resize();
		}
	}
}

