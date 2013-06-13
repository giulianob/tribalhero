package src.UI.Components.BattleReport
{
	import flash.display.*;
	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.ext.*;
	import org.aswing.geom.*;
	import src.*;
	import src.Map.*;
	import src.Objects.Factories.*;
	import src.Objects.Prototypes.*;
	import src.Objects.Troop.*;
	import src.UI.LookAndFeel.*;
	import src.UI.Tooltips.*;

	public class BattleEventTroopGridList extends GridList
	{
		public function BattleEventTroopGridList(units:*)
		{
			var validUnits: Array = [];
			for each (var unit: * in units) {
				// TODO: Don't show dead units. In the future this should be changed to show the delta.
				if (unit.count == 0) { 
					continue;
				}
				
				validUnits.push(unit);
			}
			
			super(new VectorListModel(), new GeneralGridListCellFactory(BattleEventTroopGridCell), Math.min(3, validUnits.length), 0);
			setTracksWidth(true);
			setBorder(new EmptyBorder(null, new Insets(8, 3, 8, 3)));
			
			setTileWidth(80);
			setTileHeight(40);
			
			(getModel() as VectorListModel).appendAll(validUnits);
		}
	}

}

