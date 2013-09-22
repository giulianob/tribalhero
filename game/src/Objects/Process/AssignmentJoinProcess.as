package src.Objects.Process 
{
    import com.codecatalyst.promise.Deferred;
    import com.codecatalyst.promise.Promise;

    import src.Global;
    import src.Map.City;
    import src.UI.Dialog.AssignmentJoinAtkDialog;
    import src.UI.Dialog.AssignmentJoinDefDialog;

    public class AssignmentJoinProcess
	{		
		private var attackDialog: AssignmentJoinAtkDialog;
		private var reinforceDialog: AssignmentJoinDefDialog;
		private var assignment: * ;
		private var isAttack: Boolean;
		private var sourceCity:City;
        private var deferred:Deferred = new Deferred();
		
		public function AssignmentJoinProcess(sourceCity: City, assignment: *) 
		{
			this.sourceCity = sourceCity;
			this.assignment = assignment;
			this.isAttack = assignment.isAttack==1;
		}
		
		public function execute(): Promise
		{
			if(isAttack) {
				attackDialog = new AssignmentJoinAtkDialog(sourceCity, onChoseUnits, assignment);
				attackDialog.show();
			} else {
				reinforceDialog = new AssignmentJoinDefDialog(sourceCity, onChoseUnits, assignment);
				reinforceDialog.show();
			}

            return deferred.promise;
		}
		
		public function onChoseUnits(sender: *): void {				
			Global.mapComm.Troop.assignmentJoin(sourceCity.id, assignment.id, isAttack?attackDialog.getTroop():reinforceDialog.getTroop());

            if (isAttack) {
                attackDialog.getFrame().dispose();
            }
            else {
                reinforceDialog.getFrame().dispose();
            }

            deferred.resolve(null);
		}		
	}

}