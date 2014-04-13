package src.Objects.Process 
{
    import src.Global;
    import src.Map.City;
    import src.UI.Dialog.UnitMoveDialog;

	public class ManageLocalTroopsProcess
	{
		private var city:City;
		
		public function ManageLocalTroopsProcess(city: City)
		{
			this.city = city;			
		}
		
		public function execute():void 
		{	
			var unitMove: UnitMoveDialog = new UnitMoveDialog(city, onManage);
			unitMove.show();
		}

		private function onManage(dialog: UnitMoveDialog):void
		{
			dialog.getFrame().dispose();
			
			Global.mapComm.Troop.moveUnitAndSetHideNewUnits(city.id, dialog.getTroop(), dialog.getHideNewUnits());
		}		
		
	}

}