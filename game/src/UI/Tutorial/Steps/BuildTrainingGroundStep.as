package src.UI.Tutorial.Steps 
{
	import flash.events.Event;
	import flash.events.TimerEvent;
	import flash.utils.Timer;
	import org.aswing.geom.IntPoint;
	import src.Global;
	import src.Map.City;
	import src.Map.CityObject;
	import src.UI.GameJSidebar;
	import src.UI.Sidebars.CursorCancel.CursorCancelSidebar;
	import src.UI.Sidebars.ObjectInfo.ObjectInfoSidebar;
	import src.UI.Tutorial.TutorialStep;
	
	/**
	 * This step does the following:
	 * - Show message telling user to build TG.
	 */
	public class BuildTrainingGroundStep extends TutorialStep 
	{
		private const TRAINING_GROUND_TYPE: int = 2201;
		
		private var timer: Timer = new Timer(200);
		
		public function BuildTrainingGroundStep() 
		{
			timer.addEventListener(TimerEvent.TIMER, onTimer);
		}		
		
		override public function execute(): void {					
			timer.start();
			onTimer();
		}
		
		private function onTimer(e: Event = null): void {
			var trainingGround: CityObject = map.cities.getByIndex(0).getStructureOfType(TRAINING_GROUND_TYPE);
			
			if (trainingGround != null)
			{
				this.complete();
				return;
			}
			
			var sidebar: GameJSidebar = Global.gameContainer.getSidebar();
			
			if (sidebar is CursorCancelSidebar || Global.gameContainer.frames.length) {
				hideAllMessages();
				return;
			}
						
			showMessageAtPosition(new IntPoint(20, 200), "TUTORIAL_BUILD_TRAINING_GROUND");
		}
		
		override public function dispose():void 
		{
			timer.stop();
			super.dispose();
		}
	}

}