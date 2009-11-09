/**
* ...
* @author Default
* @version 0.1
*/
	
package src.UI.Dialog {
	import flash.display.*
	import org.aswing.JFrame;
	import src.Global;
	import src.Map.*;
	import src.Objects.*;
	import src.UI.Components.TroopGridList.TroopGridList;
	import src.UI.GameJPanel;
	
	public class TroopInfoDialog extends GameJPanel {
		
		private var troopObj: TroopObject;
		
		public function TroopInfoDialog(troopObj: TroopObject):void
		{									
			title = "Unit Information";
			
			this.troopObj = troopObj;
			
			var tilelists: Array = TroopGridList.getGridList(troopObj.troop, troopObj.template);
			
			append(TroopGridList.stackGridLists(tilelists));
		}		
		
		public function show(owner:* = null, modal:Boolean = true, onClose:Function = null):JFrame 
		{
			super.showSelf(owner, modal, onClose);
			Global.gameContainer.showFrame(frame);
			return frame;
		}
	}
	
}
