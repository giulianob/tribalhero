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
	import src.Objects.Effects.Formula;
	import src.Objects.GameObject;
	import src.Objects.Troop.TroopStub;
	import src.UI.Components.TroopStubGridList.TroopStubGridCell;
	import src.UI.Cursors.GroundAttackCursor;
	import src.UI.Dialog.AssignmentCreateDialog;
	import src.UI.Dialog.AttackTroopDialog;
	import src.UI.Sidebars.CursorCancel.CursorCancelSidebar;
	/**
	 * ...
	 * @author Giuliano Barberi
	 */
	public class AssignmentCreateProcess implements IProcess
	{		
		private var attackDialog: AttackTroopDialog;		
		private var target: GameObject;
		
		public function AssignmentCreateProcess() 
		{
			
		}
		
		public function execute(): void
		{
			attackDialog = new AttackTroopDialog(onChoseUnits);
			
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
		
		public function onChoseTarget(sender: GroundAttackCursor): void 
		{						
			target = sender.getTargetObject();
			
			Global.gameContainer.setOverlaySprite(null);
			Global.gameContainer.setSidebar(null);
			
			var troop: TroopStub = attackDialog.getTroop();
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
			
			attackDialog.show();
		}
		
		public function onChoseTime(assignmentDialog: AssignmentCreateDialog): void
		{
			assignmentDialog.getFrame().dispose();
			
			Global.mapComm.Troop.assignmentCreate(Global.gameContainer.selectedCity.id, target.cityId, target.objectId, assignmentDialog.getTime(), attackDialog.getMode(), attackDialog.getTroop());
		}
	}

}