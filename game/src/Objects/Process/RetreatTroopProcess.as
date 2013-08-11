package src.Objects.Process 
{

    import src.Global;
    import src.Map.City;
    import src.Objects.Troop.TroopStub;
    import src.UI.Dialog.RetreatTroopDialog;

    public class RetreatTroopProcess implements IProcess
	{
		private var troop:TroopStub;
		
		public function RetreatTroopProcess(troop: TroopStub) 
		{
			this.troop = troop;
		}
		
		public function execute():void {
            var city: City = Global.map.cities.get(troop.cityId);

            var retreatDialog: RetreatTroopDialog = new RetreatTroopDialog(troop, onChoseUnits);

            retreatDialog.show();
        }

        public function onChoseUnits(sender: RetreatTroopDialog): void {
            sender.getFrame().dispose();

            Global.mapComm.Troop.retreat(troop.cityId, troop.id, sender.shouldRetreatAll(), sender.getTroop());
        }
		
	}

}