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
	 * - Show message telling user to build Lumbermill.
	 */
	public class BuildLumbermillStep extends TutorialStep 
	{
		private const TOWNCENTER_TYPE: int = 2000;
		private const LUMBERMILL_TYPE: int = 2107;
		
		private var timer: Timer = new Timer(200);
		
		public function BuildLumbermillStep() 
		{
			timer.addEventListener(TimerEvent.TIMER, onTimer);
		}
		
		override public function execute(): void {					
			timer.start();
			onTimer();
		}
		
		private function onTimer(e: Event = null): void {
			// If user has a lumbermill, this step is done
			var lumbermill: CityObject = map.cities.getByIndex(0).getStructureOfType(LUMBERMILL_TYPE);
			if (lumbermill != null) 
			{
				if (lumbermill.level > 0) {
					this.complete();
					return;
				}
				
				showMessageAtPosition(new IntPoint(20, 200), "TUTORIAL_WAITING_FOR_LUMBERMILL");
				return;
			}
			
			var sidebar: GameJSidebar = Global.gameContainer.getSidebar();
			
			if (sidebar is CursorCancelSidebar || Global.gameContainer.frames.length) {
				hideAllMessages();
				return;
			}
			
			// If build sidebar is up then tell user to build the lumbermill
			var objectInfoSidebar: ObjectInfoSidebar = sidebar as ObjectInfoSidebar;			
			if (objectInfoSidebar && objectInfoSidebar.gameObject.type == TOWNCENTER_TYPE) {				
				showMessageAtPosition(new IntPoint(20, 200), "TUTORIAL_CLICK_BUILD_LUMBERMILL");
				return;
			}
			
			showMessageAtPosition(new IntPoint(20, 200), "TUTORIAL_CLICK_TOWNCENTER_TO_BUILD_LUMBERMILL");
		}
		
		override public function dispose():void 
		{
			timer.stop();
			super.dispose();
		}
	}

}