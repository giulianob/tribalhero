package src.UI.Components.SimpleTroopGridList
{
    import flash.display.DisplayObject;
    import flash.events.Event;

    import org.aswing.AsWingConstants;
    import org.aswing.Insets;
    import org.aswing.JPanel;
    import org.aswing.JScrollPane;
    import org.aswing.JTabbedPane;
    import org.aswing.SoftBoxLayout;
    import org.aswing.VectorListModel;
    import org.aswing.border.EmptyBorder;
    import org.aswing.ext.GeneralGridListCellFactory;
    import org.aswing.ext.GridList;
    import org.aswing.ext.GridListItemEvent;
    import org.aswing.geom.IntDimension;

    import src.Objects.Factories.SpriteFactory;

    import src.Objects.Factories.UnitFactory;
    import src.Objects.Prototypes.UnitPrototype;
    import src.Objects.Troop.*;
    import src.UI.Tooltips.Tooltip;
    import src.UI.Tooltips.UnitTooltip;

    /**
	 *
	 * The SimpleTroopGridList is used when the grid list only needs to look at the unit template from the city for the levels.
	 * This is usually used for the drag and drop unit lists and the other basic uses.
	 *
	 * @author Giuliano
	 */
	public class SimpleTroopGridList extends GridList
	{
		private var templates: UnitTemplateManager;
		private var formationType: int;
		private var tooltip: Tooltip;
		
		public static const UNIT_CHANGED: String = "UNIT_CHANGED";

		public function SimpleTroopGridList(templateManager: UnitTemplateManager, formationType: int)
		{
			super(new VectorListModel(), new GeneralGridListCellFactory(SimpleTroopGridCell), 0, 2);
			setBorder(new EmptyBorder(null, new Insets(8, 3, 8, 3)));
			setTracksWidth(true);
			this.templates = templateManager;
			this.formationType = formationType;

			setTileWidth(70);
			setTileHeight(36);

			addEventListener(GridListItemEvent.ITEM_ROLL_OVER, onItemRollOver);
			addEventListener(GridListItemEvent.ITEM_ROLL_OUT, onItemRollOut);
		}

		public function onItemRollOver(event: GridListItemEvent):void
		{
			var dp: SimpleTroopGridCell = event.getCell() as SimpleTroopGridCell;

			var unit: Unit = dp.getCellValue().data;

			var template: UnitTemplate = templates.get(unit.type);
			var level: int = 1;
			if (template != null)
			level = template.level;

			var unitPrototype: UnitPrototype = UnitFactory.getPrototype(unit.type, level);

			if (unitPrototype)
			{
				var unitTooltip: UnitTooltip = new UnitTooltip(unitPrototype, unit.count);
				unitTooltip.show(this);
			}

			this.tooltip = unitTooltip;
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

		public function addUnit(type: int, count: int) : SimpleTroopGridCell {					
			for (var i: int = 0; i < getModel().getSize(); i++)
			{
				var troopCell: SimpleTroopGridCell = getCellByIndex(i) as SimpleTroopGridCell;
				var value: * = troopCell.getCellValue();
				if (value.data.type == type) {
					value.data.count += count;
					troopCell.setCellValue(value);
					dispatchEvent(new Event(UNIT_CHANGED));
					return null;
				}
			}

			var unit: Unit = new Unit(type, count);
			var template: UnitTemplate = templates.get(type);
			var level: int = 1;
			if (template != null)
			level = template.level;

			var unitPrototype: UnitPrototype = UnitFactory.getPrototype(type, level);
			var icon: DisplayObject = SpriteFactory.getFlashSprite(UnitFactory.getSpriteName(type, level));

			(getModel() as VectorListModel).append( { source: icon, name: (unitPrototype != null ? unitPrototype.getName() : unit.type), data: unit, level: level } );

			pack();

			dispatchEvent(new Event(UNIT_CHANGED));
			
			return getCellByIndex(getModel().getSize() - 1) as SimpleTroopGridCell;
		}

		public static function getGridList(srcTroop: TroopStub, templateManager: UnitTemplateManager, formations: Array = null): Array
		{
			//make a copy of troop
			var troop: TroopStub = new TroopStub(srcTroop.id, srcTroop.playerId, srcTroop.cityId);
			for each(var formation: Formation in srcTroop)
			{
				var newFormation: Formation = new Formation(formation.type);
				for each(var unit: Unit in formation)
				{
					newFormation.add(new Unit(unit.type, unit.count));
				}
				troop.add(newFormation);
			}

			var tilelists: Array = [];

			for each(formation in troop)
			{
				if (formations != null)
				{
					var found: Boolean = false;
					for each (var formationType: int in formations)
					{
						if (formationType == formation.type)
						{
							found = true;
							break;
						}
					}
					if (!found) continue;
				}

				var ts: SimpleTroopGridList = new SimpleTroopGridList(templateManager, formation.type);

				for (var z: int = 0; z < formation.size(); z++)
				{
					unit = formation.getByIndex(z);

					var level: int = 1;
					
					if (templateManager) {
						var template: UnitTemplate = templateManager.get(unit.type);					
						if (template != null)
							level = template.level;
					}

					var unitPrototype: UnitPrototype = UnitFactory.getPrototype(unit.type, level);
					var icon: DisplayObject = SpriteFactory.getFlashSprite(UnitFactory.getSpriteName(unit.type, level));

					(ts.getModel() as VectorListModel).append( { source: icon, name: (unitPrototype != null ? unitPrototype.getName() : unit.type), data: unit, level: level } );
				}

				tilelists.push(ts);
			}

			return tilelists;
		}

        public static function stackGridLists(tilelists: Array, includeFormationName: Boolean = true) : JPanel {
			var panel: JPanel = new JPanel();

			var layout0: SoftBoxLayout = new SoftBoxLayout();
			layout0.setAxis(AsWingConstants.VERTICAL);
			layout0.setGap(10);

			panel.setLayout(layout0);

			for each(var ts: SimpleTroopGridList in tilelists)
			{
				if (includeFormationName) {
					var tabHolder: JTabbedPane = new JTabbedPane();
					tabHolder.appendTab(new JScrollPane(ts, JScrollPane.SCROLLBAR_AS_NEEDED, JScrollPane.SCROLLBAR_NEVER), Formation.TypeStrings[ts.getFormation().type]);
					panel.append(tabHolder);
				} else {
					ts.setBorder(new EmptyBorder());
					panel.append(new JScrollPane(ts, JScrollPane.SCROLLBAR_AS_NEEDED, JScrollPane.SCROLLBAR_NEVER));
				}

				ts.setPreferredSize(new IntDimension(300, 100));
			}

			return panel;
		}
	}

}

