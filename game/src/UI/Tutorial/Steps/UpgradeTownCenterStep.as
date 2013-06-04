package src.UI.Tutorial.Steps 
{
	import flash.events.*;
	import flash.utils.*;
	import org.aswing.geom.*;
	import src.*;
	import src.Map.*;
	import src.UI.*;
	import src.UI.Sidebars.CursorCancel.*;
	import src.UI.Tutorial.*;
	
	/**
	 * This step does the following:
	 * - Show message telling user to upgrade it.
	 */
	public class UpgradeTownCenterStep extends TutorialStep 
	{
		private const TOWNCENTER_TYPE: int = 2000;
		
		private var timer: Timer = new Timer(200);
		
		public function UpgradeTownCenterStep() 
		{
			timer.addEventListener(TimerEvent.TIMER, onTimer);
		}		
		
		override public function execute(): void {					
			timer.start();
			onTimer();
		}
		
		private function onTimer(e: Event = null): void {
			var towncenter: CityObject = map.cities.getByIndex(0).getStructureOfType(TOWNCENTER_TYPE);
			if (towncenter.level >= 2) 
			{
				this.complete();
				return;
			}
			
			var sidebar: GameJSidebar = Global.gameContainer.getSidebar();
			
			if (Global.gameContainer.frames.length) {
				hideAllMessages();
				return;
			}				
			
			showMessageAtPosition(new IntPoint(20, 200), "TUTORIAL_UPGRADE_TC");
		}
		
		override public function dispose():void 
		{
			timer.stop();
			super.dispose();
		}
	}

}