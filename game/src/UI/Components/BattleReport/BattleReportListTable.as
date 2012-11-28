package src.UI.Components.BattleReport 
{
	import src.UI.Components.DynamicTable;
	import src.Util.StringHelper;
	import org.aswing.event.TableCellEditEvent;
	import org.aswing.JTable;
	import org.aswing.table.GeneralTableCellFactory;
	import org.aswing.table.PropertyTableModel;
	import org.aswing.table.TableColumn;
	import org.aswing.VectorListModel;
	
	public class BattleReportListTable extends DynamicTable 
	{
		public static const COLUMN_DATE: int = 0;
		public static const COLUMN_DATE_UNREAD_BOLD: int = 1;
		public static const COLUMN_ATTACK_TRIBES: int = 2;
		public static const COLUMN_DEFENSE_TRIBES: int = 3;
		public static const COLUMN_SIDE: int = 4;
		public static const COLUMN_LOCATION: int = 5;
		public static const COLUMN_TROOP_NAME: int = 6;
				
		private static const columnsDef: Array = [
			{ name: "REPORT_DATE_COLUMN", property: "date", cellType: DatePlainTextCell, width: 115  },
			{ name: "REPORT_DATE_COLUMN", property: ".", cellType: DateUnreadTextCell, width: 115 },
			{ name: "REPORT_ATTACKING_TRIBES_COLUMN", property: "attackerTribes", cellType: TribesReportTableCell, width: 0.5 },
			{ name: "REPORT_DEFENDING_TRIBES_COLUMN", property: "defenderTribes", cellType: TribesReportTableCell, width: 0.5 },
			{ name: "REPORT_SIDE_COLUMN", property: "side", width: 90 },
			{ name: "REPORT_LOCATION_COLUMN", property: "location", width: 0.3, cellType: BattleLocationTableCell },
			{ name: "REPORT_TROOP_COLUMN", property: "troop", width: 0.3 }
		];
		
		override protected function getColumnDefs(): Array {
			return columnsDef;
		}
		
		public function BattleReportListTable(columns: Array) 
		{
			super(columns);
		}
	}

}