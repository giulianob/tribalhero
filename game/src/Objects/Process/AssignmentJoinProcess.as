package src.Objects.Process 
{
	import adobe.utils.CustomActions;
	import src.Global;
	import src.Objects.GameObject;
	import src.UI.Dialog.AssignmentJoinAtkDialog;
	import src.UI.Dialog.AssignmentJoinDefDialog;

	public class AssignmentJoinProcess implements IProcess
	{		
		private var attackDialog: AssignmentJoinAtkDialog;
		private var reinforceDialog: AssignmentJoinDefDialog;
		private var assignment: * ;
		private var isAttack: Boolean;
		
		public function AssignmentJoinProcess(assignment: *) 
		{
			this.assignment = assignment;
			this.isAttack = assignment.isAttack==1;
		}
		
		public function execute(): void 
		{
			if(isAttack) {
				attackDialog = new AssignmentJoinAtkDialog(onChoseUnits, assignment)
				attackDialog.show();
			} else {
				reinforceDialog = new AssignmentJoinDefDialog(onChoseUnits, assignment)
				reinforceDialog.show();
			}
		}
		
		public function onChoseUnits(sender: *): void {				
			Global.mapComm.Troop.assignmentJoin(Global.gameContainer.selectedCity.id, assignment.id, isAttack?attackDialog.getTroop():reinforceDialog.getTroop());
		}		
	}

}