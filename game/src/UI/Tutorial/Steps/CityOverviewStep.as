package src.UI.Tutorial.Steps 
{
	import flash.events.Event;
	import flash.events.TimerEvent;
	import flash.filters.GlowFilter;
	import flash.utils.Timer;
	import org.aswing.event.FrameEvent;
	import org.aswing.geom.IntPoint;
	import src.Global;
	import src.Map.City;
	import src.Map.CityObject;
	import src.UI.Dialog.CityEventDialog;
	import src.UI.GameJSidebar;
	import src.UI.Sidebars.CursorCancel.CursorCancelSidebar;
	import src.UI.Sidebars.ObjectInfo.ObjectInfoSidebar;
	import src.UI.Tutorial.TutorialStep;
	
	/**
	 * This step does the following:
	 * - Show message telling user to click on the City Overview Button
	 * - Show message wizard telling user about the city overview
	 * - Tell user to wait for farm to finish
	 */
	public class CityOverviewStep extends TutorialStep 
	{		
		private const FARM_TYPE: int = 2106;
		
		private var timer: Timer = new Timer(200);
		private var openedCityOverview: Boolean = false;
		private var resourceDescriptions: int = 0;
        private var doneReadingOverview: Boolean = false;        
		
		public function CityOverviewStep() 
		{
			timer.addEventListener(TimerEvent.TIMER, onTimer);
		}		
		
		override public function execute(): void {		
			var farm: CityObject = map.cities.getByIndex(0).getStructureOfType(FARM_TYPE);
			if (farm != null && farm.level >= 1) 
			{
				this.complete();
				return;
			}            
						
			timer.start();
			onTimer();
		}
		
		private function onTimer(e: Event = null): void {						
			var overviewDialog: CityEventDialog = Global.gameContainer.findDialog(CityEventDialog);
			
			if (overviewDialog != null) {
                Global.gameContainer.btnCityInfo.filters = [];
                
				if (!this.openedCityOverview) {				
					this.openedCityOverview = true;
								
					showWizardAtPosition(new IntPoint(20, 200), "CITY_OVERVIEW", [ "CITY_OVERVIEW_MSG1", "CITY_OVERVIEW_MSG2", "CITY_OVERVIEW_MSG3", "CITY_OVERVIEW_MSG4", "CITY_OVERVIEW_MSG5", "CITY_OVERVIEW_MSG6" ]);
				}
				return;
			}
            else if (this.openedCityOverview) {
                this.doneReadingOverview = true;
            }
			
			// If user's farm has completed, this step is done
			var farm: CityObject = map.cities.getByIndex(0).getStructureOfType(FARM_TYPE);
			if (farm != null && farm.level >= 1) 
			{
                if (this.doneReadingOverview) {
                    this.complete();
                }
                
				return;
			}
			
			// If user has closed the overview dialog, show a waiting message
			if (this.doneReadingOverview) {
				showMessageAtPosition(new IntPoint(20, 200), "TUTORIAL_WAIT_FOR_FARM");
			}
			else {				
				if (Global.gameContainer.btnCityInfo.filters.length == 0) {
					Global.gameContainer.btnCityInfo.filters = [
						new GlowFilter(0xFFFFFF, 1, 8, 8, 5)
					];
				}
				showMessageAtPosition(new IntPoint(Global.gameContainer.btnCityInfo.x, Global.gameContainer.btnCityInfo.y + Global.gameContainer.btnCityInfo.height + 15), "TUTORIAL_CLICK_CITY_OVERVIEW");			
			}
		}
		
		override public function dispose():void 
		{
            Global.gameContainer.btnCityInfo.filters = [];
			timer.stop();
			super.dispose();
		}	
	}

}