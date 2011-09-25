package src.Objects.Process 
{
	import src.Global;
	import src.Objects.GameObject;
	import src.UI.Dialog.AssignmentJoinDialog;
	/**
	 * ...
	 * @author Giuliano Barberi
	 */
	public class AssignmentJoinProcess implements IProcess
	{		
		private var attackDialog: AssignmentJoinDialog;
		private var assignment: * ;
		
		public function AssignmentJoinProcess(assignment: *) 
		{
			this.assignment = assignment;
		}
		
		public function execute(): void 
		{
			attackDialog = new AssignmentJoinDialog(onChoseUnits, assignment)
			
			attackDialog.show();
		}
		
		public function onChoseUnits(sender: AssignmentJoinDialog): void {
			
			Global.gameContainer.closeAllFrames(true);
				
			Global.mapComm.Troop.assignmentJoin(Global.gameContainer.selectedCity.id, assignment.id, attackDialog.getTroop());

			Global.gameContainer.setOverlaySprite(null);
			Global.gameContainer.setSidebar(null);
		}		
	}

}