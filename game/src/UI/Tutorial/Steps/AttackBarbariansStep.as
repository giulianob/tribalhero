package src.UI.Tutorial.Steps 
{
    import flash.events.Event;
    import flash.events.TimerEvent;
    import flash.utils.Timer;

    import org.aswing.geom.IntPoint;

    import src.Global;
    import src.Map.City;
    import src.Objects.Actions.Action;
    import src.UI.Dialog.InfoDialog;
    import src.UI.GameJSidebar;
    import src.UI.Sidebars.BarbarianTribeInfo.BarbarianTribeSidebar;
    import src.UI.Sidebars.CursorCancel.CursorCancelSidebar;
    import src.UI.Tutorial.TutorialStep;
    import src.Util.StringHelper;

    /**
	 * This step does the following:
	 * - Show message telling user to find the barbarian tribe
	 * - Show message telling user to attack barbarian tribe
	 */
	public class AttackBarbariansStep extends TutorialStep 
	{
		private var timer: Timer = new Timer(200);
		
		public function AttackBarbariansStep() 
		{
			timer.addEventListener(TimerEvent.TIMER, onTimer);
		}		
		
		override public function execute(): void {					
			timer.start();
			onTimer();
		}
		
		private function onTimer(e: Event = null): void {
			// If user has an attack action then this step is done
			var city: City = map.cities.getByIndex(0);
			var hasAttackTroops: Boolean = city.currentActions.getActions(Action.BARBARIAN_TRIBE_ATTACK_CHAIN).length > 0;

			if (hasAttackTroops) 
			{
				InfoDialog.showMessageDialog("Tribal Hero", StringHelper.localize("TUTORIAL_END"));
				this.complete();
				return;
			}
			
			var sidebar: GameJSidebar = Global.gameContainer.getSidebar();
			
			if (sidebar is CursorCancelSidebar || Global.gameContainer.frames.length) {
				hideAllMessages();
				return;
			}
			
			// If barbarian sidebar is up
			var barbarianTribeSidebar: BarbarianTribeSidebar = sidebar as BarbarianTribeSidebar;			
			if (barbarianTribeSidebar) {				
				showMessageAtPosition(new IntPoint(20, 200), "TUTORIAL_ATTACK_BARBARIANS");
				return;
			}
			
			showMessageAtPosition(new IntPoint(20, 200), "TUTORIAL_FIND_BARBARIANS");
		}
		
		override public function dispose():void 
		{
			timer.stop();
			super.dispose();
		}
	}

}