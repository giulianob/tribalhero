package src.Objects.Process 
{
	import flash.events.Event;
	import org.aswing.JButton;
	import src.Global;
	import src.Objects.GameObject;
	import src.Objects.Location;
	import src.Objects.SimpleGameObject;
	import src.Objects.Stronghold.Stronghold;
	import src.Objects.StructureObject;
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
		private var location : Location;
		
		public function ReinforcementSendProcess(location: Location = null) 
		{
			this.location = location;
		}
		
		public function execute(): void 
		{
			reinforceDialog = new ReinforceTroopDialog(onChoseUnits,true);
			
			reinforceDialog.show();
		}
		
		public function onChoseUnits(sender: ReinforceTroopDialog): void {
			
			Global.gameContainer.closeAllFrames(true);
			
			
			if(location==null) {
				var sidebar: CursorCancelSidebar = new CursorCancelSidebar();
				
				var cursor: GroundReinforceCursor = new GroundReinforceCursor(onChoseTarget, reinforceDialog.getTroop());
				
				var changeTroop: JButton = new JButton("Change Troop");
				changeTroop.addActionListener(onChangeTroop);
				sidebar.append(changeTroop);
				
				Global.gameContainer.setSidebar(sidebar);
			} else {
				sendReinforcement(location.type, location.id);
				Global.gameContainer.setOverlaySprite(null);
				Global.gameContainer.setSidebar(null);
			}
		}
		
		private function sendReinforcement(type : int, id : uint): void {
			if (type == Location.CITY) {
				Global.mapComm.Troop.troopReinforceCity(Global.gameContainer.selectedCity.id, id, reinforceDialog.getTroop(), reinforceDialog.getMode());
			}
			else if (type == Location.STRONGHOLD) {
				Global.mapComm.Troop.troopReinforceStronghold(Global.gameContainer.selectedCity.id, id, reinforceDialog.getTroop(), reinforceDialog.getMode());
			}			
		}
		public function onChoseTarget(sender: GroundReinforceCursor): void {			
			
			var target: SimpleGameObject = sender.getTargetObject();

			if (target is StructureObject) {
				sendReinforcement(Location.CITY, target.groupId);
			}
			else if (target is Stronghold) {
				sendReinforcement(Location.STRONGHOLD, target.objectId);
			}

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