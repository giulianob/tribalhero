package src.Objects.Process 
{
    import org.aswing.JOptionPane;

    import src.Global;
    import src.Objects.Troop.TroopStub;
    import src.UI.Dialog.InfoDialog;

    public class RetreatTroopProcess implements IProcess
	{
		private var troop:TroopStub;
		
		public function RetreatTroopProcess(troop: TroopStub) 
		{
			this.troop = troop;
			
		}
		
		public function execute():void 
		{
			InfoDialog.showMessageDialog("Confirm", "Are you sure? Retreating will bring your troop back to your city..", function(result: int): void {				
				if (result == JOptionPane.YES) {
					Global.mapComm.Troop.retreat(troop.cityId, troop.id);
				}
				
			}, null, true, true, JOptionPane.YES | JOptionPane.NO);					
		}
		
	}

}