package src.UI.Components.TroopsDialogTable 
{
    import src.UI.Components.DynamicTable;

    public class TroopTable extends DynamicTable
	{		
		public static const COLUMN_NAME: int = 0;
		public static const COLUMN_LOCATION: int = 1;
		public static const COLUMN_STATUS: int = 2;
		public static const COLUMNS_UNITS: int = 3;
		public static const COLUMN_ACTIONS: int = 4;
        
        // These columns should be used when adding the troop notifications rather than the actual troop stubs
        public static const COLUMN_NOTIFICATION_NAME: int = 5;
        public static const COLUMN_NOTIFICATION_LOCATION: int = 6;
        public static const COLUMN_NOTIFICATION_STATUS: int = 7;
        public static const COLUMN_NOTIFICATION_ACTIONS: int = 8;
		
		private static const columnsDef: Array = [
			{ name: "TROOP_NAME_COLUMN", property: ".", cellType: TroopNameCell, width: 125  },
			{ name: "TROOP_LOCATION_COLUMN", property: ".", cellType: TroopLocationCell, width: 200 },
			{ name: "TROOP_STATUS_COLUMN", property: ".", cellType: TroopStatusCell, width: 0.3 },
			{ name: "TROOP_UNITS_COLUMN", property: ".", cellType: TroopUnitsCell, width: 0.5 },
            { name: "", property: ".", cellType: TroopActionsCell , width: 155 },
            
            { name: "TROOP_NOTIFICATION_NAME_COLUMN", property: ".", cellType: TroopNotificationNameCell, width: 125 },
            { name: "TROOP_NOTIFICATION_LOCATION_COLUMN", property: ".", cellType: TroopNotificationLocationCell, width: 200 },
            { name: "TROOP_NOTIFICATION_STATUS_COLUMN", property: ".", cellType: TroopNotificationStatusCell, width: 0.5 },
            { name: "", property: ".", cellType: TroopNotificationActionsCell, width: 60 },
            			
		];
		
		public function TroopTable(columns:Array) 
		{
			super(columns);			
		}		
		
		override protected function getColumnDefs(): Array {
			return columnsDef;
		}
						
	}
}