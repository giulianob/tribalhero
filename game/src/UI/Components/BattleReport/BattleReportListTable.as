package src.UI.Components.BattleReport 
{
	import fl.lang.Locale;
	import org.aswing.event.TableCellEditEvent;
	import org.aswing.JTable;
	import org.aswing.table.GeneralTableCellFactory;
	import org.aswing.table.PropertyTableModel;
	import org.aswing.table.TableColumn;
	import org.aswing.VectorListModel;
	
	public class BattleReportListTable extends JTable 
	{
		public static const COLUMN_DATE: int = 0;
		public static const COLUMN_DATE_UNREAD_BOLD: int = 1;
		public static const COLUMN_ATTACK_TRIBES: int = 2;
		public static const COLUMN_DEFENSE_TRIBES: int = 3;
		public static const COLUMN_SIDE: int = 4;
		public static const COLUMN_LOCATION: int = 5;
		public static const COLUMN_TROOP_NAME: int = 6;
		
		private static const columnsDef: Array = [
			{ name: "REPORT_DATE_COLUMN", property: "date", width: 115  },
			{ name: "REPORT_DATE_COLUMN", property: ".", cellType: DateUnreadTextCell, width: 115 },
			{ name: "REPORT_ATTACKING_TRIBES_COLUMN", property: "attackerTribes", cellType: TribesReportTableCell, width: 0.5 },
			{ name: "REPORT_DEFENDING_TRIBES_COLUMN", property: "defenderTribes", cellType: TribesReportTableCell, width: 0.5 },
			{ name: "REPORT_SIDE_COLUMN", property: "side", width: 90 },
			{ name: "REPORT_LOCATION_COLUMN", property: "location", width: 0.3, cellType: BattleLocationTableCell },
			{ name: "REPORT_TROOP_COLUMN", property: "troop", width: 0.3 }
		];
		
		private var combinedWeight:Number = 0;
		private var fixedWidth:Number = 0;
		private var columns:Array;
		private var lastCWidth: int;
		
		public function BattleReportListTable(columns: Array) 
		{					
			this.columns = columns;
			addEventListener(TableCellEditEvent.EDITING_STARTED, function(e: TableCellEditEvent) : void {
				getCellEditor().cancelCellEditing();
			});			
			
			var names: Array = [];
			var properties: Array = [];
			var translators: Array = [];
			
			var columnIdx: int;
			for each (columnIdx in columns) {
				names.push(Locale.loadString(columnsDef[columnIdx].name));
				properties.push(columnsDef[columnIdx].property);
				translators.push(null);
				if (columnsDef[columnIdx].width < 1) {
					combinedWeight += columnsDef[columnIdx].width;
				}
				else {
					fixedWidth += columnsDef[columnIdx].width;
				}
			}
			
			var vector: VectorListModel = new VectorListModel();
			
			var model: PropertyTableModel = new PropertyTableModel(vector, names, properties, translators);
			
			super(model);
			
			getTableHeader().setResizingAllowed(false);
			
			setSelectionMode(JTable.SINGLE_SELECTION);
			
			setAutoResizeMode(JTable.AUTO_RESIZE_OFF);					
			
			for (var i: int = 0; i < columns.length; i++) {		
				columnIdx = columns[i];
				var colDef: * = columnsDef[columnIdx];
				
				var column: TableColumn = getColumnAt(i);
							
				if (!colDef.hasOwnProperty("cellType")) {
					continue;
				}
				
				column.setCellFactory(new GeneralTableCellFactory(colDef.cellType));
			}			
		}
		
		override public function doLayout():void 
		{	
			var cWidth:int = getWidth() - getInsets().getMarginWidth();			
			
			if (cWidth > 0) {
				if (cWidth != lastCWidth) {

					cWidth = cWidth - 15 - fixedWidth;
					var remainingWidth: int = cWidth;
					
					for (var i: int = 0; i < columns.length; i++) {		
						var columnIdx: int = columns[i];				
						var colDef: * = columnsDef[columnIdx];				
						var column: TableColumn = getColumnAt(i);						
						var colWidth: int;
						
						if (colDef.width < 1) {
							colWidth = cWidth / (combinedWeight / colDef.width);							
						}
						else {
							colWidth = colDef.width;
						}						
						
						if (i + 1 == columns.length) {
							colWidth = Math.max(remainingWidth, colWidth);
						}																				
						
						column.setWidth(0);
						column.setMinWidth(colWidth);				
						column.setMaxWidth(colWidth);		
						remainingWidth -= colWidth;
					}
					
					lastCWidth = cWidth;
				}
			}
			
			super.doLayout();						
		}
	}

}