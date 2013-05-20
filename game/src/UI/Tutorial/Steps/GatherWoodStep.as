package src.UI.Tutorial.Steps 
{
	import flash.geom.Point;
	import src.Map.MapUtil;
	import src.Util.StringHelper;
	import flash.events.Event;
	import flash.events.TimerEvent;
	import flash.utils.Timer;
	import org.aswing.geom.IntPoint;
	import src.Global;
	import src.Map.City;
	import src.Map.CityObject;
	import src.Objects.Actions.Action;
	import src.UI.Dialog.InfoDialog;
	import src.UI.GameJSidebar;
	import src.UI.Sidebars.CursorCancel.CursorCancelSidebar;
	import src.UI.Sidebars.ForestInfo.ForestInfoSidebar;
	import src.UI.Sidebars.ObjectInfo.ObjectInfoSidebar;
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
					
					var mainBuildingScreenPos: Point = MapUtil.getScreenCoord(city.MainBuilding.x, city.MainBuilding.y);
					if (!Global.gameContainer.camera.CameraRectangle().containsPoint(mainBuildingScreenPos)) {
						return;
					}					
				}
				
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
			
			showMessageAtPosition(new IntPoint(20, 200), "TUTORIAL_FIND_FOREST");
		}
		
		override public function dispose():void 
		{
			timer.stop();
			super.dispose();
		}
	}

}