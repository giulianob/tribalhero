package src.UI.Components.TroopCompositionGridList
{
	import flash.display.*;
	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.ext.*;
	import src.Map.*;
	import src.Objects.Factories.*;
	import src.Objects.Prototypes.*;
	import src.Objects.Troop.*;
	import src.UI.Tooltips.*;

		public class TroopCompositionGridList extends GridList
	{
		private var city: City;

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
					var unitPrototype: UnitPrototype = UnitFactory.getPrototype(unit.type, 1);
					var icon: DisplayObject = UnitFactory.getSprite(unit.type, 1, false) as DisplayObject;
					icon.scaleX = 0.5;
					icon.scaleY = 0.5;
					(getModel() as VectorListModel).append( { source: icon, data: unit } );
				}

			}
		}


	}

}

