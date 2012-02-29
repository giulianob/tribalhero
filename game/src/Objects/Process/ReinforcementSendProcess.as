package src.Objects.Process 
{
	import flash.events.Event;
	import org.aswing.JButton;
	import src.Global;
	import src.Objects.GameObject;
	import src.UI.Cursors.GroundReinforceCursor;
	import src.UI.Dialog.ReinforceTroopDialog;
	import src.UI.Sidebars.CursorCancel.CursorCancelSidebar;
	/**
	 * ...
	 * @author Giuliano Barberi
	 */
	public class ReinforcementSendProcess implements IProcess
	{		
		private var reinforceDialog: ReinforceTroopDialog;
		
		public function ReinforcementSendProcess() 
		{
			
		}
		
		public function execute(): void 
		{
			reinforceDialog = new ReinforceTroopDialog(onChoseUnits,true);
			
			reinforceDialog.show();
		}
		
		public function onChoseUnits(sender: ReinforceTroopDialog): void {
			
			Global.gameContainer.closeAllFrames(true);
			
			var sidebar: CursorCancelSidebar = new CursorCancelSidebar();
			
			var cursor: GroundReinforceCursor = new GroundReinforceCursor(onChoseTarget, reinforceDialog.getTroop());
			
			var changeTroop: JButton = new JButton("Change Troop");
			changeTroop.addActionListener(onChangeTroop);
			sidebar.append(changeTroop);
			
			Global.gameContainer.setSidebar(sidebar);
		}
		
		public function onChoseTarget(sender: GroundReinforceCursor): void {			
			
			var target: GameObject = sender.getTargetObject();
			
			Global.mapComm.Troop.troopReinforce(Global.gameContainer.selectedCity.id, target.cityId, reinforceDialog.getTroop(), reinforceDialog.getMode());

			Global.gameContainer.setOverlaySprite(null);
			Global.gameContainer.setSidebar(null);
		}
		
		public function onChangeTroop(e: Event = null): void {
			Global.gameContainer.setOverlaySprite(null);
			Global.gameContainer.setSidebar(null);
			
			reinforceDialog.show();
		}
	}

}