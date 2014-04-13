package src.UI.Components.TroopCompositionGridList
{
    import flash.display.*;
    import flash.utils.Dictionary;

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

        private var formationType: int;

		public function TroopCompositionGridList(troop: TroopStub, col: int, row: int)
		{
			super(new VectorListModel(), new GeneralGridListCellFactory(TroopCompositionGridCell), col, row);
			setBorder(new EmptyBorder(null, new Insets(0, 0, 0, 0)));
			setTracksWidth(true);
			this.formationType = formationType;

			setHGap(0);		
			setTileWidth(55);
			setTileHeight(20);
			setHorizontalAlignment(AsWingConstants.LEFT);
			setVerticalAlignment(AsWingConstants.TOP);
			
			if (troop) {
				setTroop(troop);
			}
		}
		
		public function setTroop(troop:TroopStub):void 
		{
			(getModel() as VectorListModel).clear();
			
			var units: Dictionary = troop.toUnitsArray();
			for (var unitType: Object in units)
			{
				var icon: DisplayObject = UnitFactory.getSprite(int(unitType), 1, false) as DisplayObject;
				icon.scaleX = 0.5;
				icon.scaleY = 0.5;
				(getModel() as VectorListModel).append( { source: icon, data: new Unit(int(unitType), units[unitType]) } );					
			}
		}
	}
}
