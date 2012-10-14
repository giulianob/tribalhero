package src.Objects.Process 
{
	import src.Util.StringHelper;
	import flash.events.Event;
	import org.aswing.JButton;
	import org.aswing.JOptionPane;
	import src.Global;
	import src.Objects.GameObject;
	import src.Objects.Location;
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
		private var location: Location;
		
		public function AttackSendProcess(location: Location = null) 
		{
			this.location = location;
		}
		
		public function execute(): void 
		{
			attackDialog = new AttackTroopDialog(onChoseUnits);			
			
			attackDialog.show();
		}
		
		public function onChoseUnits(sender: AttackTroopDialog): void {
			
			Global.gameContainer.closeAllFrames(true);
			
			if(location==null) {
				var sidebar: CursorCancelSidebar = new CursorCancelSidebar();
				
				var cursor: GroundAttackCursor = new GroundAttackCursor(onChoseTarget, attackDialog.getTroop());
				
				var changeTroop: JButton = new JButton("Change Troop");
				changeTroop.addActionListener(onChangeTroop);
				sidebar.append(changeTroop);
				
				Global.gameContainer.setSidebar(sidebar);
			} else {
				if (location.type==Location.CITY) {
					Global.mapComm.City.isCityUnderAPBonus(location.id, onGotAPStatus);
				}
				else {
					onAttackAccepted();
				}
			}
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
				InfoDialog.showMessageDialog(StringHelper.localize("STR_MESSAGE"), StringHelper.localize("SEND_ATTACK_AP_BONUS"), function (result: int): void {
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
			if(location==null) {
				if (target is StructureObject) {
					Global.mapComm.Troop.troopAttackCity(Global.gameContainer.selectedCity.id, target.groupId, target.objectId, attackDialog.getMode(), attackDialog.getTroop(), onAttackFail);
				}
				else if (target is Stronghold) {
					Global.mapComm.Troop.troopAttackStronghold(Global.gameContainer.selectedCity.id, target.objectId, attackDialog.getMode(), attackDialog.getTroop(), onAttackFail);				
				}
			} else {
				if (location.type==Location.CITY) {
					Global.mapComm.Troop.troopAttackCity(Global.gameContainer.selectedCity.id, location.id, location.objectId, attackDialog.getMode(), attackDialog.getTroop(), onAttackFail);
				}
				else if (location.type==Location.STRONGHOLD) {
					Global.mapComm.Troop.troopAttackStronghold(Global.gameContainer.selectedCity.id, location.id, attackDialog.getMode(), attackDialog.getTroop(), onAttackFail);				
				}
			}

			Global.gameContainer.setOverlaySprite(null);
			Global.gameContainer.setSidebar(null);
		}
		
		public function onAttackFail(custom: * = null):void {
			if(location==null) {
				onChoseUnits(attackDialog);
			}
		}
		
		public function onChangeTroop(e: Event = null): void {
			Global.gameContainer.setOverlaySprite(null);
			Global.gameContainer.setSidebar(null);
			
			attackDialog.show();
		}
	}

}