
package src.UI.Sidebars.ObjectInfo.Buttons {
	import flash.display.MovieClip;
	import flash.display.SimpleButton;
	import flash.events.Event;
	import flash.events.MouseEvent;
	import src.Constants;
	import src.Global;
	import src.Map.City;
	import src.Map.Map;
	import src.Objects.*;
	import src.Objects.Actions.ActionButton;
	import src.Objects.Prototypes.UnitPrototype;
	import src.UI.Cursors.*;
	import src.UI.Dialog.LaborMoveDialog;
	import src.UI.Dialog.NumberInputDialog;
	import src.UI.Tooltips.TextTooltip;
	
	public class LaborMoveButton extends ActionButton
	{							
		private var textToolTip: TextTooltip;
		
		public function LaborMoveButton(btn: SimpleButton, parentObj: GameObject) 
		{			
			super(btn, parentObj);
			
			textToolTip = new TextTooltip("Assign Workers");
			
			ui.addEventListener(MouseEvent.CLICK, onMouseClick);
			ui.addEventListener(MouseEvent.MOUSE_OVER, onMouseOver);
			ui.addEventListener(MouseEvent.MOUSE_MOVE, onMouseOver);
			ui.addEventListener(MouseEvent.MOUSE_OUT, onMouseOut);						
		}
		
		public function onMouseOver(event: MouseEvent):void
		{			
			textToolTip.show(this);
		}
		
		public function onMouseOut(event: MouseEvent):void
		{
			textToolTip.hide();
		}
		
		public function onMouseClick(MouseEvent: Event):void
		{
			if (enabled)
			{
				var inputDialog: LaborMoveDialog = new LaborMoveDialog(parentObj as StructureObject, onAcceptDialog);				
				inputDialog.show();
			}
		}
		
		public override function validateButton():Boolean 
		{
			return enabled;
		}
		
		public function onAcceptDialog(sender: LaborMoveDialog):void
		{
			Global.map.mapComm.Object.laborMove(this.parentObj.cityId, this.parentObj.objectId, sender.getCount());
			sender.getFrame().dispose();
		}
	}
	
}