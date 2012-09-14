package src.UI.Components.TroopCompositionGridList
{
	import flash.display.DisplayObject;
	import org.aswing.AsWingConstants;
	import org.aswing.border.EmptyBorder;
	import org.aswing.border.SimpleTitledBorder;
	import org.aswing.ext.GeneralGridListCellFactory;
	import org.aswing.ext.GridList;
	import org.aswing.ext.GridListItemEvent;
	import org.aswing.geom.IntDimension;
	import org.aswing.Insets;
	import org.aswing.JPanel;
	import org.aswing.JTabbedPane;
	import org.aswing.SoftBoxLayout;
	import org.aswing.VectorListModel;
	import src.Global;
	import src.Map.City;
	import src.Objects.Factories.UnitFactory;
	import src.Objects.Prototypes.UnitPrototype;
	import src.Objects.Troop.*;
	import src.UI.Components.TroopStubGridList.TroopStubGridCell;
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

	public class TroopCompositionGridList extends GridList
	{
		private var city: City;
		//The template can either be UnitTemplate or TroopTemplate and the code will act accordingly
		private var template: *;
		private var formationType: int;
		private var tooltip: Tooltip;

		public function TroopCompositionGridList(troop: TroopStub, formationType: int, col: int, row: int)
		{
			super(new VectorListModel(), new GeneralGridListCellFactory(TroopCompositionGridCell), col, row);
			setBorder(new EmptyBorder(null, new Insets(0, 0, 0, 0)));
			setTracksWidth(true);
			this.formationType = formationType;

			setTileWidth(40);
			setTileHeight(32);			
			
			for each(var formation: Formation in troop.each())
			{
				//Don't show empty formations for tooltips
				if (formation.type!=formationType || formation.size() == 0) continue;

				for (var z: int = 0; z < formation.size(); z++)
				{
					var unit: Unit = formation.getByIndex(z);
					trace("t:" + unit.type + "," + unit.count );
					var unitPrototype: UnitPrototype = UnitFactory.getPrototype(unit.type, 1);
					var icon: DisplayObject = UnitFactory.getSprite(unit.type, 1, false) as DisplayObject;
					icon.scaleX = 0.5;
					icon.scaleY = 0.5;
					(getModel() as VectorListModel).append( { source: icon, data: unit} );
				}

			}
		}


	}

}

