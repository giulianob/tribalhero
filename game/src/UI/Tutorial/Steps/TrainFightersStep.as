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
	import src.Objects.Prototypes.TechnologyPrototype;
	import src.Objects.TechnologyStats;
	import src.UI.GameJSidebar;
	import src.UI.Sidebars.CursorCancel.CursorCancelSidebar;
	import src.UI.Sidebars.ObjectInfo.ObjectInfoSidebar;
	import src.UI.Tutorial.TutorialStep;
	import System.Linq.Enumerable;
	
	/**
	 * This step does the following:
	 * - Show message telling user to train Fighters.
	 * - Waits until fighters are trained before continuing
	 */
	public class TrainFightersStep extends TutorialStep 
	{	
		private const FIGHTER_TYPE: int = 11;
		
		private var timer: Timer = new Timer(200);
		
		public function TrainFightersStep() 
		{
			timer.addEventListener(TimerEvent.TIMER, onTimer);
		}		
		
		override public function execute(): void {										
			timer.start();
			onTimer();
		}
		
		private function onTimer(e: Event = null): void {
			// If user has at least 15 fighters or sends out an attack against a barb tribe then this step is done
			var city: City = map.cities.getByIndex(0);
			var fighterCount: int = city.troops.getIndividualUnitCount(FIGHTER_TYPE);
			var hasTrainingAction: Boolean = city.currentActions.getActions(Action.UNIT_TRAIN).length > 0;
			var hasAttackTroops: Boolean = city.currentActions.getActions(Action.BARBARIAN_TRIBE_ATTACK_CHAIN).length > 0;
			var fightersNeeded: int = Math.max(0, 15 - fighterCount);

			if (fightersNeeded == 0 || hasAttackTroops)
			{
				this.complete();
				return;
			}
			
			var sidebar: GameJSidebar = Global.gameContainer.getSidebar();
			
			if (sidebar is CursorCancelSidebar || Global.gameContainer.frames.length) {
				hideAllMessages();
				return;
			}			
			
			if (hasTrainingAction) {
				showMessageAtPosition(new IntPoint(20, 200), "TUTORIAL_WAIT_FOR_FIGHTERS");
			}
            else if (fighterCount > 0) {
                showMessageAtPosition(new IntPoint(20, 200), "TUTORIAL_TRAIN_MORE_FIGHTER", fightersNeeded);
            }
			else {
				showMessageAtPosition(new IntPoint(20, 200), "TUTORIAL_TRAIN_FIGHTER");
			}			
		}
		
		override public function dispose():void 
		{
			timer.stop();
			super.dispose();
		}
	}

}