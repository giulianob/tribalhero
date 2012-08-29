package src.Objects.Process 
{
	import fl.lang.Locale;
	import flash.events.Event;
	import org.aswing.JButton;
	import org.aswing.JOptionPane;
	import src.Global;
	import src.Objects.GameObject;
	import src.Objects.SimpleGameObject;
	import src.Objects.Stronghold.Stronghold;
	import src.Objects.StructureObject;
	import src.UI.Cursors.GroundAttackCursor;
	import src.UI.Dialog.AttackTroopDialog;
	import src.UI.Dialog.InfoDialog;
	import src.UI.Sidebars.CursorCancel.CursorCancelSidebar;

	public class AttackSendProcess implements IProcess
	{		
		private var attackDialog: AttackTroopDialog;		
		private var target: SimpleGameObject;
		
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
			this.target = sender.getTargetObject();
			
			if (target is StructureObject) {
				Global.mapComm.City.isCityUnderAPBonus(target.groupId, onGotAPStatus);
			}
			else {
				onAttackAccepted();
			}
		}
		
		public function onGotAPStatus(hasBonuses: Boolean): void {
			if (!hasBonuses) {
				onAttackAccepted();
			}
			else {
				InfoDialog.showMessageDialog(Locale.loadString("STR_MESSAGE"), Locale.loadString("SEND_ATTACK_AP_BONUS"), function (result: int): void {
					if (result == JOptionPane.YES) {
						onAttackAccepted();
					}
					else {
						onAttackFail();
					}
				}, null, true, false, JOptionPane.YES | JOptionPane.NO);
			}
		}
		
		public function onAttackAccepted(): void {				
			if (target is StructureObject) {
				Global.mapComm.Troop.troopAttackCity(Global.gameContainer.selectedCity.id, target.groupId, target.objectId, attackDialog.getMode(), attackDialog.getTroop(), onAttackFail);
			}
			else if (target is Stronghold) {
				Global.mapComm.Troop.troopAttackStronghold(Global.gameContainer.selectedCity.id, target.objectId, attackDialog.getMode(), attackDialog.getTroop(), onAttackFail);				
			}

			Global.gameContainer.setOverlaySprite(null);
			Global.gameContainer.setSidebar(null);
		}
		
		public function onAttackFail(custom: * = null):void {
			onChoseUnits(attackDialog);
		}
		
		public function onChangeTroop(e: Event = null): void {
			Global.gameContainer.setOverlaySprite(null);
			Global.gameContainer.setSidebar(null);
			
			attackDialog.show();
		}
	}

}