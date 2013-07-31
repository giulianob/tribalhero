package src.Objects.Process 
{
    import flash.events.Event;

    import org.aswing.JButton;

    import src.Global;
    import src.Map.City;
    import src.Map.Position;
    import src.Map.TileLocator;
    import src.Objects.Effects.Formula;
    import src.Objects.SimpleGameObject;
    import src.Objects.Stronghold.Stronghold;
    import src.Objects.StructureObject;
    import src.Objects.Troop.TroopStub;
    import src.UI.Cursors.GroundReinforceCursor;
    import src.UI.Dialog.AssignmentCreateDialog;
    import src.UI.Dialog.ReinforceTroopDialog;
    import src.UI.Sidebars.CursorCancel.CursorCancelSidebar;

    public class DefAssignmentCreateProcess implements IProcess
	{		
		private var troopDialog: ReinforceTroopDialog;
		private var target: SimpleGameObject;
		private var sourceCity:City;
		
		public function DefAssignmentCreateProcess(sourceCity: City) 
		{
			this.sourceCity = sourceCity;
			
		}
		
		public function execute(): void
		{
			troopDialog = new ReinforceTroopDialog(sourceCity, onChoseUnits);
			
			troopDialog.show();
		}
		
		public function onChoseUnits(sender: ReinforceTroopDialog): void 
		{			
			Global.gameContainer.closeAllFrames(true);
			
			var sidebar: CursorCancelSidebar = new CursorCancelSidebar();
			
			new GroundReinforceCursor(sourceCity, onChoseTarget, troopDialog.getTroop());
			
			var changeTroop: JButton = new JButton("Change Troop");
			changeTroop.addActionListener(onChangeTroop);
			sidebar.append(changeTroop);
			
			Global.gameContainer.setSidebar(sidebar);
		}
		
		public function onChoseTarget(sender: GroundReinforceCursor): void 
		{						
			target = sender.getTargetObject();
			
			Global.gameContainer.setOverlaySprite(null);
			Global.gameContainer.setSidebar(null);
			
			var troop: TroopStub = troopDialog.getTroop();
			var targetMapDistance: Position = target.primaryPosition.toPosition();
			var distance: int = TileLocator.distance(sourceCity.MainBuilding.x, sourceCity.MainBuilding.y, 1, targetMapDistance.x, targetMapDistance.y, 1);
			
			var assignmentDialog: AssignmentCreateDialog = new AssignmentCreateDialog(Formula.moveTimeTotal(sourceCity, troop.getSpeed(sourceCity), distance, true), onChoseTime);
			assignmentDialog.show();
		}
		
		public function onChangeTroop(e: Event = null): void
		{
			Global.gameContainer.setOverlaySprite(null);
			Global.gameContainer.setSidebar(null);
			
			troopDialog.show();
		}
		
		public function onChoseTime(assignmentDialog: AssignmentCreateDialog): void
		{
			assignmentDialog.getFrame().dispose();
			if (target is StructureObject) {
				Global.mapComm.Troop.cityAssignmentCreate(sourceCity.id, target.groupId, target.objectId, assignmentDialog.getTime(), troopDialog.getMode(), troopDialog.getTroop(), assignmentDialog.getDescription(),false);
			} else if (target is Stronghold) {
				Global.mapComm.Troop.strongholdAssignmentCreate(sourceCity.id, target.objectId, assignmentDialog.getTime(), troopDialog.getMode(), troopDialog.getTroop(), assignmentDialog.getDescription(), false);
			}
		}
	}

}