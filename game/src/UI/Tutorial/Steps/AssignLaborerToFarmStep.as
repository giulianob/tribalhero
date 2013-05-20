package src.UI.Tutorial.Steps 
{
	import flash.events.Event;
	import flash.events.TimerEvent;
	import flash.utils.Timer;
	import org.aswing.geom.IntPoint;
	import src.Global;
	import src.Map.City;
	import src.Map.CityObject;
	import src.Objects.Actions.Action;
	import src.UI.GameJSidebar;
	import src.UI.Sidebars.CursorCancel.CursorCancelSidebar;
	import src.UI.Sidebars.ObjectInfo.ObjectInfoSidebar;
	import src.UI.Tutorial.TutorialStep;
	
	/**
	 * This step does the following:
	 * - Show message telling user to click Farm.
	 * - Show message telling user to assign laborers.
	 */
	public class AssignLaborerToFarmStep extends TutorialStep 
	{
		private const FARM_TYPE: int = 2106;
		
		private var timer: Timer = new Timer(200);
		
		public function AssignLaborerToFarmStep() 
		{
			timer.addEventListener(TimerEvent.TIMER, onTimer);
		}		
		
		override public function execute(): void {		
			timer.start();
			onTimer();
		}
		
		private function onTimer(e: Event = null): void {
			// If user has a farm with laborers or an assign laborer action is in progress, then this action is done
			var farm: CityObject = map.cities.getByIndex(0).getStructureOfType(FARM_TYPE);
			if (farm != null) 
			{
				if (farm.labor > 0 || farm.city.currentActions.hasAction(Action.LABOR_MOVE, farm.objectId)) {
					this.complete();
					return;
				}
			}
			
			var sidebar: GameJSidebar = Global.gameContainer.getSidebar();
			
			if (sidebar is CursorCancelSidebar || Global.gameContainer.frames.length) {
				hideAllMessages();
				return;
			}
			
			// If build sidebar is up then tell user to assign laborers
			var objectInfoSidebar: ObjectInfoSidebar = sidebar as ObjectInfoSidebar;			
			if (objectInfoSidebar && objectInfoSidebar.gameObject.type == FARM_TYPE) {				
				showMessageAtPosition(new IntPoint(20, 200), "TUTORIAL_CLICK_FARM_ASSIGN_LABORERS");
				return;
			}
			
			showMessageAtPosition(new IntPoint(20, 200), "TUTORIAL_CLICK_FARM");
		}
		
		override public function dispose():void 
		{
			timer.stop();
			super.dispose();
		}
	}

}