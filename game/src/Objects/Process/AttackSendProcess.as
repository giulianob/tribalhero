package src.Objects.Process 
{
	import flash.events.Event;
	import org.aswing.JButton;
	import src.Global;
	import src.Objects.GameObject;
	import src.UI.Cursors.GroundAttackCursor;
	import src.UI.Dialog.AttackTroopDialog;
	import src.UI.Sidebars.CursorCancel.CursorCancelSidebar;
	/**
	 * ...
	 * @author Giuliano Barberi
	 */
	public class AttackSendProcess implements IProcess
	{		
		private var attackDialog: AttackTroopDialog;
		
		public function AttackSendProcess() 
		{
			
		}
		
		public function execute(): void 
		{
			attackDialog = new AttackTroopDialog(onChoseUnits);			
			
			attackDialog.show();
		}
		
		public function onChoseUnits(sender: AttackTroopDialog): void {
			
			Global.gameContainer.closeAllFrames(true);
			
			var sidebar: CursorCancelSidebar = new CursorCancelSidebar();
			
			var cursor: GroundAttackCursor = new GroundAttackCursor(onChoseTarget, attackDialog.getTroop());
			
			var changeTroop: JButton = new JButton("Change Troop");
			changeTroop.addActionListener(onChangeTroop);
			sidebar.append(changeTroop);
			
			Global.gameContainer.setSidebar(sidebar);
		}
		
		public function onChoseTarget(sender: GroundAttackCursor): void {			
			
			var target: GameObject = sender.getTargetObject();
			
			Global.mapComm.Troop.troopAttack(Global.gameContainer.selectedCity.id, target.cityId, target.objectId, attackDialog.getMode(), attackDialog.getTroop(), onAttackFail);

			Global.gameContainer.setOverlaySprite(null);
			Global.gameContainer.setSidebar(null);
		}
		
		public function onAttackFail(custom: *):void {
			onChoseUnits(attackDialog);
		}
		
		public function onChangeTroop(e: Event = null): void {
			Global.gameContainer.setOverlaySprite(null);
			Global.gameContainer.setSidebar(null);
			
			attackDialog.show();
		}
	}

}