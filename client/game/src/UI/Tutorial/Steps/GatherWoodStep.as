package src.UI.Tutorial.Steps 
{
    import flash.events.Event;
    import flash.events.TimerEvent;
    import flash.filters.GlowFilter;
    import flash.geom.Point;
    import flash.utils.Timer;

    import org.aswing.geom.IntPoint;

    import src.Global;
    import src.Map.City;
    import src.Map.CityObject;
    import src.Map.ScreenPosition;
    import src.UI.GameJSidebar;
    import src.UI.Sidebars.ForestInfo.ForestInfoSidebar;
    import src.UI.Tutorial.TutorialStep;

    /**
	 * This step does the following:
	 * - Show message telling user to find a level 1 Forest.
	 * - Show message telling user to gather wood from Forest.
	 */
	public class GatherWoodStep extends TutorialStep 
	{		
		private const FORESTCAMP_TYPE: int = 2108;
		
		private var shouldShowTutorialEndMsg: Boolean = false;
		
		private var timer: Timer = new Timer(200);
		
		public function GatherWoodStep() 
		{
			timer.addEventListener(TimerEvent.TIMER, onTimer);
		}		
		
		override public function execute(): void {								
			timer.start();			
			onTimer();
		}
		
		private function onTimer(e: Event = null): void {
			// If user has a forest
			var city: City = map.cities.getByIndex(0);
			var forestCamp: CityObject = city.getStructureOfType(FORESTCAMP_TYPE);
			if (forestCamp != null) {
				
				if (shouldShowTutorialEndMsg) {
					showMessageAtPosition(new IntPoint(20, 200), "TUTORIAL_BACK_TO_CITY_AFTER_FOREST");
					
					var mainBuildingScreenPos: ScreenPosition = city.MainBuilding.primaryPosition.toScreenPosition();
                    var mainBuildingPoint: Point = new Point(mainBuildingScreenPos.x, mainBuildingScreenPos.y);
					if (!Global.gameContainer.camera.cameraRectangle().containsPoint(mainBuildingPoint)) {
						return;
					}					
				}
				
				Global.gameContainer.minimapTools.btnMinimapZoom.filters = [];
				this.complete();
				return;				
			}				
			
			shouldShowTutorialEndMsg = true;
			
			var sidebar: GameJSidebar = Global.gameContainer.getSidebar();
			
			if (Global.gameContainer.frames.length) {
				hideAllMessages();
				return;
			}
			
			// If build sidebar is up then tell user to assign laborers
			var forestInfoSidebar: ForestInfoSidebar = sidebar as ForestInfoSidebar;			
			if (forestInfoSidebar) {
				showMessageAtPosition(new IntPoint(20, 200), "TUTORIAL_CLICK_GATHER_WOOD");
				return;
			}
			
			if (Global.gameContainer.minimapTools.btnMinimapZoom.filters.length == 0) {
				Global.gameContainer.minimapTools.btnMinimapZoom.filters = [
					new GlowFilter(0xFFFFFF, 1, 8, 8, 5)
				];
			}
				
			showMessageAtPosition(new IntPoint(20, 200), "TUTORIAL_FIND_FOREST");
		}
		
		override public function dispose():void 
		{
			timer.stop();
			super.dispose();
		}
	}

}