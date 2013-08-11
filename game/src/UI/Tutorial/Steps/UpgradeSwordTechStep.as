package src.UI.Tutorial.Steps 
{
    import System.Linq.Enumerable;

    import flash.events.Event;
    import flash.events.TimerEvent;
    import flash.utils.Timer;

    import org.aswing.geom.IntPoint;

    import src.Global;
    import src.Map.City;
    import src.Map.CityObject;
    import src.Objects.TechnologyStats;
    import src.UI.GameJSidebar;
    import src.UI.Sidebars.CursorCancel.CursorCancelSidebar;
    import src.UI.Tutorial.TutorialStep;

    /**
	 * This step does the following:
	 * - Show message telling user to train Sword Tech.
	 */
	public class UpgradeSwordTechStep extends TutorialStep 
	{
		private const TRAINING_GROUND_TYPE: int = 2201;
		private const BASIC_TECH_TYPE: int = 22015;
		
		private var timer: Timer = new Timer(200);
		
		public function UpgradeSwordTechStep() 
		{
			timer.addEventListener(TimerEvent.TIMER, onTimer);
		}		
		
		override public function execute(): void {					
			if (map.cities.size() > 1) {
				this.complete();
				return;
			}
						
			timer.start();
			onTimer();
		}
		
		private function onTimer(e: Event = null): void {
			// If user has basic training tech then this step is done
			var city: City = map.cities.getByIndex(0);			
			var trainingGround: CityObject = city.getStructureOfType(TRAINING_GROUND_TYPE);
			var hasSwordTech: Boolean = Enumerable.from(trainingGround.techManager.technologies).any(function (tech: TechnologyStats): Boolean {
				return tech.techPrototype.techtype == BASIC_TECH_TYPE;
			});					
						
			if (hasSwordTech) 
			{
				this.complete();
				return;
			}
			
			var sidebar: GameJSidebar = Global.gameContainer.getSidebar();
			
			if (sidebar is CursorCancelSidebar || Global.gameContainer.frames.length) {
				hideAllMessages();
				return;
			}			

			showMessageAtPosition(new IntPoint(20, 200), "TUTORIAL_RESEARCH_BASIC_TRAINING_TECH_CLICK_TG");
		}
		
		override public function dispose():void 
		{
			timer.stop();
			super.dispose();
		}
	}

}