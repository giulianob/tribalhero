package src.Objects.Process 
{
	import flash.events.Event;
	import flash.geom.Point;
	import org.aswing.JAdjuster;
	import org.aswing.JButton;
	import org.aswing.plaf.basic.BasicAdjusterUI;
	import org.aswing.UIDefaults;
	import src.Global;
	import src.Map.City;
	import src.Map.MapUtil;
	import src.Objects.BarbarianTribe;
	import src.Objects.Effects.Formula;
	import src.Objects.GameObject;
	import src.Objects.SimpleGameObject;
	import src.Objects.Stronghold.Stronghold;
	import src.Objects.StructureObject;
	import src.Objects.Troop.TroopStub;
	import src.UI.Components.TroopStubGridList.TroopStubGridCell;
	import src.UI.Cursors.GroundAttackCursor;
	import src.UI.Dialog.AssignmentCreateDialog;
	import src.UI.Dialog.AttackTroopDialog;
	import src.UI.Dialog.InfoDialog;
	import src.UI.Sidebars.CursorCancel.CursorCancelSidebar;
	import src.Util.StringHelper;
	
	public class AtkAssignmentCreateProcess implements IProcess
	{		
		private var attackDialog: AttackTroopDialog;		
		private var target: SimpleGameObject;
		private var sourceCity:City;
		
		public function AtkAssignmentCreateProcess(sourceCity: City) 
		{
			this.sourceCity = sourceCity;
			
		}
		
		public function execute(): void
		{
			attackDialog = new AttackTroopDialog(sourceCity, onChoseUnits);
			
			attackDialog.show();
		}
		
		public function onChoseUnits(sender: AttackTroopDialog): void 
		{			
			Global.gameContainer.closeAllFrames(true);
			
			var sidebar: CursorCancelSidebar = new CursorCancelSidebar();
			
			var cursor: GroundAttackCursor = new GroundAttackCursor(onChoseTarget, attackDialog.getTroop());
			
			var changeTroop: JButton = new JButton("Change Troop");
			changeTroop.addActionListener(onChangeTroop);
			sidebar.append(changeTroop);
			
			Global.gameContainer.setSidebar(sidebar);
		}
		
		private function onBadTarget(response: *) : void
		{
			onChoseUnits(attackDialog);
		}
		
		public function onChoseTarget(sender: GroundAttackCursor): void 
		{						
			target = sender.getTargetObject();
			if (target is BarbarianTribe)
			{
				InfoDialog.showMessageDialog("Error", StringHelper.localize("BARBARIAN_ASSIGNMENT_ERROR"),onBadTarget);
				return;
			}
            
			Global.gameContainer.setOverlaySprite(null);
			Global.gameContainer.setSidebar(null);
			
			var troop: TroopStub = attackDialog.getTroop();			
			var targetMapDistance: Point = MapUtil.getMapCoord(target.objX, target.objY);
			var distance: int = sourceCity.MainBuilding.distance(targetMapDistance.x, targetMapDistance.y);
			
			var assignmentDialog: AssignmentCreateDialog = new AssignmentCreateDialog(Formula.moveTimeTotal(sourceCity, troop.getSpeed(sourceCity), distance, true), onChoseTime);
			assignmentDialog.show();
		}
		
		public function onChangeTroop(e: Event = null): void
		{
			Global.gameContainer.setOverlaySprite(null);
			Global.gameContainer.setSidebar(null);
			
			attackDialog.show();
		}
		
		public function onChoseTime(assignmentDialog: AssignmentCreateDialog): void
		{
			assignmentDialog.getFrame().dispose();
			if (target is StructureObject) {
				Global.mapComm.Troop.cityAssignmentCreate(sourceCity.id, target.groupId, target.objectId, assignmentDialog.getTime(), attackDialog.getMode(), attackDialog.getTroop(), assignmentDialog.getDescription(),true);
			} else if (target is Stronghold) {
				Global.mapComm.Troop.strongholdAssignmentCreate(sourceCity.id, target.objectId, assignmentDialog.getTime(), attackDialog.getMode(), attackDialog.getTroop(), assignmentDialog.getDescription(),true);
			}
		}
	}

}