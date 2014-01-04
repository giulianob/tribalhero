package src.UI.Tooltips {
    import org.aswing.*;

    import src.Map.City;
    import src.Objects.Factories.UnitFactory;
    import src.Objects.Prototypes.UnitPrototype;
    import src.Objects.Troop.TroopTemplate;
    import src.Objects.Troop.TroopTemplateManager;
    import src.Objects.Troop.Unit;
    import src.Objects.Troop.UnitTemplate;
    import src.Objects.Troop.UnitTemplateManager;
    import src.UI.Components.UnitStatBox;
    import src.UI.LookAndFeel.GameLookAndFeel;
    import src.Util.BinaryList.BinaryList;

    public class ComplexUnitTooltip extends Tooltip{

		private var unit: Unit;
		private var city: City;
		private var template: BinaryList;

		private var pnlHeader: JPanel;
		private var pnlFooter: JPanel;
		private var lblName: JLabel;
		private var lblUpkeep: JLabel;
		private var lblLevel: JLabel;
		private var lblInfo: JLabel;
		private var statsBox: UnitStatBox;

		public function ComplexUnitTooltip(unit: Unit, city: City, template: BinaryList) {
			this.unit = unit;
			this.city = city;
			this.template = template;

			createUI();

			var unitPrototype: UnitPrototype;
			if (template is UnitTemplateManager) {
				var unitTemplate: UnitTemplate = template.get(unit.type);
				unitPrototype = UnitFactory.getPrototype(unitTemplate.type, unitTemplate.level);
			}
			else {
				var troopTemplate: TroopTemplate = template.get(unit.type);
				unitPrototype = UnitFactory.getPrototype(troopTemplate.type, troopTemplate.level);
			}

			lblName.setText(unit.count + " " + unitPrototype.getName(unit.count));
			lblUpkeep.setText("-" + unitPrototype.upkeep * unit.count);
			lblLevel.setText("Level " + unitPrototype.level);
			lblInfo.setText("Stats for a single " + unitPrototype.getName().toLowerCase());
		}

		private function createUI() : void {
			ui.setMinimumWidth(125);
			ui.setLayout(new SoftBoxLayout(AsWingConstants.VERTICAL, 15));
			pnlHeader = new JPanel(new BorderLayout(20, 0));
			pnlFooter = new JPanel(new SoftBoxLayout(AsWingConstants.VERTICAL, 0));

			lblName = new JLabel();
			lblName.setHorizontalAlignment(AsWingConstants.LEFT);
			lblName.setConstraints("West");
			GameLookAndFeel.changeClass(lblName, "header");

			lblUpkeep = new JLabel();
			lblUpkeep.setConstraints("East");
			lblUpkeep.setHorizontalAlignment(AsWingConstants.RIGHT);
			lblUpkeep.setIcon(new AssetIcon(new ICON_CROP()));
			lblUpkeep.setHorizontalTextPosition(AsWingConstants.LEFT);
			lblUpkeep.setIconTextGap(0);
			GameLookAndFeel.changeClass(lblUpkeep, "Tooltip.text");

			lblLevel = new JLabel();
			lblLevel.setHorizontalAlignment(AsWingConstants.LEFT);
			lblLevel.setConstraints("South");
			GameLookAndFeel.changeClass(lblLevel, "Tooltip.text");

			lblInfo = new JLabel();
			lblInfo.setHorizontalAlignment(AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(lblInfo, "Tooltip.text");

			if (template is TroopTemplateManager) 
				statsBox = UnitStatBox.createFromTroopTemplate(unit.type, template as TroopTemplateManager);
			else
				statsBox = UnitStatBox.createFromCityTemplate(unit.type, city);

			pnlHeader.append(lblName);
			pnlHeader.append(lblUpkeep);			

			pnlHeader.append(lblLevel);

			pnlFooter.append(lblInfo);
			pnlFooter.append(statsBox);

			ui.append(pnlHeader);
			ui.append(pnlFooter);
		}

	}
}

