package src.UI.Components.CombatObjectGridList
{
    import flash.display.DisplayObject;

    import org.aswing.Insets;
    import org.aswing.VectorListModel;
    import org.aswing.border.EmptyBorder;
    import org.aswing.ext.GeneralGridListCellFactory;
    import org.aswing.ext.GridList;
    import org.aswing.ext.GridListItemEvent;

    import src.Objects.Battle.CombatObject;
    import src.Objects.Factories.ObjectFactory;
    import src.Objects.Prototypes.StructurePrototype;
    import src.Objects.Prototypes.UnitPrototype;
    import src.UI.Tooltips.StructureTooltip;
    import src.UI.Tooltips.Tooltip;
    import src.UI.Tooltips.UnitTooltip;
    import src.Util.Util;

	public class CombatObjectGridList extends GridList
	{
		private var tooltip: Tooltip;

		public function CombatObjectGridList()
		{
			super(new VectorListModel(), new GeneralGridListCellFactory(CombatObjectGridCell), 0, 2);
			setBorder(new EmptyBorder(null, new Insets(8, 3, 8, 3)));
			setTracksWidth(true);

			setTileWidth(66);
			setTileHeight(60);

			addEventListener(GridListItemEvent.ITEM_ROLL_OVER, onItemRollOver);
			addEventListener(GridListItemEvent.ITEM_ROLL_OUT, onItemRollOut);
		}

		public function onItemRollOver(event: GridListItemEvent):void
		{
			var objPrototype: * = event.getValue().prototype;

			if (objPrototype is UnitPrototype)
			{
				var unitTooltip: UnitTooltip = new UnitTooltip(objPrototype);
				unitTooltip.show(event.getCell().getCellComponent());

				this.tooltip = unitTooltip;
			}
			else if (objPrototype is StructurePrototype)
			{
				var structureTooltip: StructureTooltip = new StructureTooltip(null, objPrototype);
				structureTooltip.show(event.getCell().getCellComponent());

				this.tooltip = structureTooltip;
			}
		}

		public function onItemRollOut(event: GridListItemEvent):void
		{
			if (tooltip)
			tooltip.hide();

			tooltip = null;
		}

		public function addCombatObject(combatObj: CombatObject):void {
			var prototype: * = ObjectFactory.getPrototype(combatObj.type, combatObj.level);
			var icon: DisplayObject = combatObj.getIcon();
			Util.resizeSprite(icon, 55, 35);

			(getModel() as VectorListModel).append( { "source": icon, "data": combatObj, "prototype": prototype} );
		}

		public static function getGridList(combatObjects: Array): CombatObjectGridList
		{
			var gridList: CombatObjectGridList = new CombatObjectGridList();

			for each (var combatObj: CombatObject in combatObjects)
			gridList.addCombatObject(combatObj);

			return gridList;
		}
	}

}
