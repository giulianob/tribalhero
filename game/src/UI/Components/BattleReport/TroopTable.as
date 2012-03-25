package src.UI.Components.BattleReport
{
	import flash.display.DisplayObject;
	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.event.TableCellEditEvent;
	import org.aswing.geom.*;
	import org.aswing.colorchooser.*;
	import org.aswing.ext.*;
	import org.aswing.table.GeneralTableCellFactory;
	import org.aswing.table.PropertyTableModel;
	import src.Objects.Factories.ObjectFactory;
	import src.UI.Components.PlayerLabel;
	import src.UI.Components.SimpleTooltip;
	
	public class TroopTable extends JPanel
	{
		private var unitList:VectorListModel;
		private var tableModel:PropertyTableModel;
		private var tblUnits:JTable;
		
		private var troop:Object;
		
		public function TroopTable(units:Array)
		{
			this.troop = troop;
			
			createUI();
					
			tblUnits.addEventListener(TableCellEditEvent.EDITING_STARTED, function(e:TableCellEditEvent):void
				{
					tblUnits.getCellEditor().cancelCellEditing();
				});
			
			unitList.appendAll(units);
		}
		
		private function createUI():void
		{
			unitList = new VectorListModel();
			tableModel = new PropertyTableModel(
				unitList, 
				["Unit", "Level", "HP", "Hits\nTaken", "Damage\nTaken", "Hits\nDealt", "Damage\nDealt"], 
				[".", "level", "hp", "hitsTaken", "damageTaken", "hitsDealt", "damageDealt"], 
				[null, null, null, null, null, null]
			);
			
			tblUnits = new JTable(tableModel);
			tblUnits.setRowSelectionAllowed(false);
			tblUnits.setRowHeight(42);
			tblUnits.getColumn("Unit").setCellFactory(new GeneralTableCellFactory(UnitIconCell));
			tblUnits.getColumn("Unit").setPreferredWidth(82);
			tblUnits.getColumn("Level").setPreferredWidth(34);
			
			tblUnits.getColumn("HP").setCellFactory(new GeneralTableCellFactory(OneDecimalValueCell));
			tblUnits.getColumn("Damage\nTaken").setCellFactory(new GeneralTableCellFactory(OneDecimalValueCell));
			tblUnits.getColumn("Damage\nDealt").setCellFactory(new GeneralTableCellFactory(OneDecimalValueCell));
			append(tblUnits);
		}
	}
}