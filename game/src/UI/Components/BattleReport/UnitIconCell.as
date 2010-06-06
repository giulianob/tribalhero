package src.UI.Components.BattleReport 
{
	import flash.display.DisplayObject;
    import org.aswing.*;
    import org.aswing.table.*;
	import src.Objects.Factories.ObjectFactory;
	import src.Objects.Prototypes.StructurePrototype;
	
	public class UnitIconCell extends DefaultTextCell
	{
		
		public function UnitIconCell() 
		{
			
		}
	
        override public function setCellValue(param1: *) : void
        {
            setText("");		
			
			setHorizontalAlignment(AsWingConstants.CENTER);
            setIcon(new AssetIcon(param1));
        }		
	}

}