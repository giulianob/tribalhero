package src.UI.Components 
{
    import org.aswing.JTable;
    import org.aswing.VectorListModel;
    import org.aswing.event.TableCellEditEvent;
    import org.aswing.table.GeneralTableCellFactory;
    import org.aswing.table.PropertyTableModel;
    import org.aswing.table.TableColumn;

    import src.Util.StringHelper;

    public class DynamicTable extends JTable
	{

		private var combinedWeight:Number = 0;
		private var fixedWidth:Number = 0;
		private var columns:Array;
		private var lastCWidth: int;
		
		public function DynamicTable(columns: Array) 
		{					
			var columnsDef: Array = getColumnDefs();
			
			this.columns = columns;
			addEventListener(TableCellEditEvent.EDITING_STARTED, function(e: TableCellEditEvent) : void {
				getCellEditor().cancelCellEditing();
			});			
			
			var names: Array = [];
			var properties: Array = [];
			var translators: Array = [];
			
			var columnIdx: int;
			for each (columnIdx in columns) {
				names.push(StringHelper.localize(columnsDef[columnIdx].name));
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
			
			setColumnSelectionAllowed(false);
			setRowSelectionAllowed(false);
		}
		
		override public function doLayout():void 
		{	
			var columnsDef: Array = getColumnDefs();
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
		
		protected function getColumnDefs(): Array {
			throw new Error("Must be overriden in subclass");
		}
	}

}