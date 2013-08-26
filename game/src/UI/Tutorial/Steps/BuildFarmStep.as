package src.UI.Tutorial.Steps 
{
    import flash.events.Event;
    import flash.events.TimerEvent;
    import flash.utils.Timer;

    import org.aswing.geom.IntPoint;

    import src.Global;
    import src.Map.CityObject;
    import src.UI.GameJSidebar;
    import src.UI.Sidebars.CursorCancel.CursorCancelSidebar;
    import src.UI.Sidebars.ObjectInfo.ObjectInfoSidebar;
    import src.UI.Tutorial.TutorialStep;

    /**
	 * This step does the following:
	 * - Show message telling user what TC is and to click it.
	 * - Show message telling user to build Farm.
	 */
	public class BuildFarmStep extends TutorialStep 
	{
		private const TOWNCENTER_TYPE: int = 2000;
		private const FARM_TYPE: int = 2106;
		
		private var timer: Timer = new Timer(200);
		
		public function BuildFarmStep() 
		{
			timer.addEventListener(TimerEvent.TIMER, onTimer);
		}		
		
		override public function execute(): void {					
			timer.start();
			onTimer();
		}
		
		private function onTimer(e: Event = null): void {
			// If user has a farm, this step is done
			var farm: CityObject = map.cities.getByIndex(0).getStructureOfType(FARM_TYPE);
			if (farm != null) 
			{
				this.complete();
				return;
			}
			
			var sidebar: GameJSidebar = Global.gameContainer.getSidebar();
			
			if (sidebar is CursorCancelSidebar || Global.gameContainer.frames.length) {
				hideAllMessages();
				return;
			}
			
			// If build sidebar is up then tell user to build the farm
			var objectInfoSidebar: ObjectInfoSidebar = sidebar as ObjectInfoSidebar;			
			if (objectInfoSidebar && objectInfoSidebar.gameObject.type == TOWNCENTER_TYPE) {				
				showMessageAtPosition(new IntPoint(20, 200), "TUTORIAL_CLICK_BUILD_FARM");
				return;
			}
			
			showMessageAtPosition(new IntPoint(20, 200), "TUTORIAL_CLICK_TOWNCENTER");
		}
		
		override public function dispose():void 
		{
			timer.stop();
			super.dispose();
		}
	}

}