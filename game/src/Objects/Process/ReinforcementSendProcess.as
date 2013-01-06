package src.Objects.Process 
{
	import flash.events.*;
	import org.aswing.*;
	import src.*;
	import src.Map.*;
	import src.Objects.*;
	import src.Objects.Stronghold.*;
	import src.UI.Cursors.*;
	import src.UI.Dialog.*;
	import src.UI.Sidebars.CursorCancel.*;

	public class ReinforcementSendProcess implements IProcess
	{		
		private var reinforceDialog: ReinforceTroopDialog;
		private var location : Location;
		private var sourceCity:City;
		
		public function ReinforcementSendProcess(sourceCity: City, targetLocation: Location = null) 
		{
			this.sourceCity = sourceCity;
			this.location = targetLocation;
		}
		
		public function execute(): void 
		{
			reinforceDialog = new ReinforceTroopDialog(sourceCity, onChoseUnits, true);
			
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
				Global.mapComm.Troop.troopReinforceCity(sourceCity.id, id, reinforceDialog.getTroop(), reinforceDialog.getMode());
			}
			else if (type == Location.STRONGHOLD) {
				Global.mapComm.Troop.troopReinforceStronghold(sourceCity.id, id, reinforceDialog.getTroop(), reinforceDialog.getMode());
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