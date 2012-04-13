package src.Objects.Process 
{
	import flash.events.Event;
	import flash.geom.Point;
	import org.aswing.JButton;
	import src.Global;
	import src.Map.City;
	import src.Map.MapUtil;
	import src.Objects.Effects.Formula;
	import src.Objects.GameObject;
	import src.Objects.Troop.TroopStub;
	import src.UI.Cursors.GroundReinforceCursor;
	import src.UI.Dialog.AssignmentCreateDialog;
	import src.UI.Dialog.ReinforceTroopDialog;
	import src.UI.Sidebars.CursorCancel.CursorCancelSidebar;

	public class DefAssignmentCreateProcess implements IProcess
	{		
		private var troopDialog: ReinforceTroopDialog;
		private var target: GameObject;
		
		public function DefAssignmentCreateProcess() 
		{
			
		}
		
		public function execute(): void
		{
			troopDialog = new ReinforceTroopDialog(onChoseUnits);
			
			troopDialog.show();
		}
		
		public function onChoseUnits(sender: ReinforceTroopDialog): void 
		{			
			Global.gameContainer.closeAllFrames(true);
			
			var sidebar: CursorCancelSidebar = new CursorCancelSidebar();
			
			var cursor: GroundReinforceCursor = new GroundReinforceCursor(onChoseTarget, troopDialog.getTroop());
			
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
			var city: City = Global.gameContainer.selectedCity;
			var targetMapDistance: Point = MapUtil.getMapCoord(target.getX(), target.getY());
			var distance: int = city.MainBuilding.distance(targetMapDistance.x, targetMapDistance.y);
			
			var assignmentDialog: AssignmentCreateDialog = new AssignmentCreateDialog(Formula.moveTimeTotal(city, troop.getSpeed(city), distance, true), onChoseTime);
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
			
			Global.mapComm.Troop.assignmentCreate(Global.gameContainer.selectedCity.id, target.cityId, target.objectId, assignmentDialog.getTime(), troopDialog.getMode(), troopDialog.getTroop(), assignmentDialog.getDescription(),false);
		}
	}

}