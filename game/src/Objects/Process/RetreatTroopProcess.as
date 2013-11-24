package src.Objects.Process 
{
    import org.aswing.JButton;
    import org.aswing.JOptionPane;

    import src.Comm.Packet;

    import src.Global;
    import src.Map.City;
    import src.Map.MapComm;
    import src.Objects.Troop.TroopStub;
    import src.UI.Dialog.InfoDialog;
    import src.UI.Dialog.RetreatTroopDialog;
    import src.Util.StringHelper;

    public class RetreatTroopProcess
	{
		private var troop:TroopStub;
        private var onFinished: Function;

		public function RetreatTroopProcess(troop: TroopStub, onFinished: Function = null)
		{
			this.troop = troop;
            this.onFinished = onFinished;
        }
		
		public function execute():void {
            var city: City = Global.map.cities.get(troop.cityId);

            // If city isn't defined its someone elses troop
            if (!city) {
                InfoDialog.showMessageDialog(
                        StringHelper.localize("STR_CONFIRM"),
                        StringHelper.localize("RETREAT_TROOP_PROCESS_RETREAT_OTHER_UNITS"),
                        onChoseRetreatAllUnits,
                        null,
                        true,
                        true,
                        JOptionPane.YES | JOptionPane.NO);

                return;
            }

            var retreatDialog: RetreatTroopDialog = new RetreatTroopDialog(troop, onChoseUnits);

            retreatDialog.show();
        }

        private function onChoseRetreatAllUnits(result: int): void {
            if (result == JOptionPane.YES) {
                Global.mapComm.Troop.retreat(troop.cityId, troop.id, true, null);
            }
        }

        public function onChoseUnits(sender: RetreatTroopDialog): void {
            sender.getFrame().dispose();

            Global.mapComm.Troop.retreat(troop.cityId, troop.id, sender.shouldRetreatAll(), sender.getTroop(), onRetreated);
        }

        private function onRetreated(packet: Packet, custom: *): void {
            if (MapComm.tryShowError(packet)) {
                return;
            }

            if (onFinished) {
                onFinished();
            }
        }
		
	}

}