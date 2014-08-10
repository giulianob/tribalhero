package src.UI.Components.ComplexTroopGridList
{
    import flash.display.DisplayObject;

    import org.aswing.AsWingConstants;
    import org.aswing.Insets;
    import org.aswing.JPanel;
    import org.aswing.JTabbedPane;
    import org.aswing.SoftBoxLayout;
    import org.aswing.VectorListModel;
    import org.aswing.border.EmptyBorder;
    import org.aswing.border.SimpleTitledBorder;
    import org.aswing.ext.GeneralGridListCellFactory;
    import org.aswing.ext.GridList;
    import org.aswing.ext.GridListItemEvent;
    import org.aswing.geom.IntDimension;

    import src.Global;
    import src.Map.City;
    import src.Objects.Factories.SpriteFactory;
    import src.Objects.Factories.UnitFactory;
    import src.Objects.Prototypes.UnitPrototype;
    import src.Objects.Troop.*;
    import src.UI.LookAndFeel.GameLookAndFeel;
    import src.UI.Tooltips.ComplexUnitTooltip;
    import src.UI.Tooltips.Tooltip;

    /**
	 *
	 * The ComplexTroopGridlist is used to show troopstubs in tooltips and panels. It will take in consideration the
	 * troop id and formation, and automatically use either the city's template or the troop's template to display the appropriate level info
	 *
	 * @author Giuliano
	 */

	public class ComplexTroopGridList extends GridList
	{
		private var city: City;
		//The template can either be UnitTemplate or TroopTemplate and the code will act accordingly
		private var template: *;
		private var formationType: int;
		private var tooltip: Tooltip;

		public function ComplexTroopGridList(city: City, template: *, formationType: int, tooltipMode: Boolean = false)
		{
			super(new VectorListModel(), new GeneralGridListCellFactory(ComplexTroopGridCell), 0, 2);
			setBorder(new EmptyBorder(null, new Insets(8, 3, 8, 3)));
			setTracksWidth(true);
			this.template = template;
			this.city = city;
			this.formationType = formationType;

			setTileWidth(65);
			setTileHeight(32);			
			setColsRows(5, -1);

			if (!tooltipMode) {
				addEventListener(GridListItemEvent.ITEM_ROLL_OVER, onItemRollOver);
				addEventListener(GridListItemEvent.ITEM_ROLL_OUT, onItemRollOut);
			}
		}

		public function onItemRollOver(event: GridListItemEvent):void
		{
			var dp: ComplexTroopGridCell = event.getCell() as ComplexTroopGridCell;

			var unit: Unit = dp.getCellValue().data;

			this.tooltip = new ComplexUnitTooltip(unit, city, template);
			this.tooltip.show(dp);
		}

		public function onItemRollOut(event: GridListItemEvent):void
		{
			if (tooltip)
			tooltip.hide();

			tooltip = null;
		}

		public function getFormation(): Formation
		{
			var formation: Formation = new Formation(formationType);

			for (var i: int = 0; i < getModel().getSize(); i++)
			{
				var currItem: * = getModel().getElementAt(i);
				formation.add(new Unit(currItem.data.type, currItem.data.count));
			}

			return formation;
		}

		public static function getGridList(troop: TroopStub, tooltipMode: Boolean = false): Array
		{
			//If we ever have this do drag and drop or modify the troop in anyway, it needs to copy it first just like in the SimpleTroopGridlist

			var tilelists: Array = [];

			for each(var formation: Formation in troop)
			{
				//Don't show empty formations for tooltips
				if (tooltipMode && formation.size() == 0) continue;

				var ts: ComplexTroopGridList;
				var template: * ;
				var city: City;
				//If this is the local troop and we are looking at either normal or garrisson, then we will use the city's template for the info
				if (troop.id == 1 && formation.type != Formation.InBattle) {
					city = Global.map.cities.get(troop.cityId);
					template = city.template;
				}
				//Otherwise, we will use the troop stub's template
				else {
					template = troop.template;
				}

				ts = new ComplexTroopGridList(city, template, formation.type, tooltipMode);

				for (var z: int = 0; z < formation.size(); z++)
				{
					var unit: Unit = formation.getByIndex(z);

					//Again, here we will check which template we are using, depending on the conditions mentioned above
					//and find the level based on the appropriate one

					var level: int = 1;
					if (template is UnitTemplateManager) {
						var unitTemplate: UnitTemplate = template.get(unit.type);
						if (unitTemplate)
							level = unitTemplate.level;
					}
					else if (template is TroopTemplateManager) {
						var troopTemplate: TroopTemplate = template.get(unit.type);
						if (troopTemplate)
							level = troopTemplate.level;
					}

					var unitPrototype: UnitPrototype = UnitFactory.getPrototype(unit.type, level);
					var icon: DisplayObject = SpriteFactory.getFlashSprite(UnitFactory.getSpriteName(unit.type, level, tooltipMode));

					(ts.getModel() as VectorListModel).append( { source: icon, name: (unitPrototype != null ? unitPrototype.getName() : unit.type), data: unit, level: level, tooltipMode: tooltipMode } );
				}

				tilelists.push(ts);
			}

			return tilelists;
		}

        public static function stackGridLists(tilelists: Array, tooltipMode: Boolean = false) : JPanel {
			var panel: JPanel = new JPanel();

			var layout0: SoftBoxLayout = new SoftBoxLayout();
			layout0.setAxis(AsWingConstants.VERTICAL);
			
			layout0.setGap(10);			

			panel.setLayout(layout0);

			for each(var ts: ComplexTroopGridList in tilelists)
			{
				if (tooltipMode) {
					ts.setBorder(new SimpleTitledBorder(null, Formation.TypeStrings[ts.getFormation().type], AsWingConstants.TOP, AsWingConstants.LEFT, 0, GameLookAndFeel.getClassAttribute("Tooltip.text", "Label.font"), GameLookAndFeel.getClassAttribute("Tooltip.text", "Label.foreground")));
					panel.append(ts);					
				}
				else {
					var tabHolder: JTabbedPane = new JTabbedPane();
					ts.setBorder(new EmptyBorder(null, new Insets()));
					tabHolder.appendTab(ts, Formation.TypeStrings[ts.getFormation().type]);
					panel.append(tabHolder);
					ts.setPreferredSize(new IntDimension(300, 75));
				}
			}

			return panel;
		}
	}

}

