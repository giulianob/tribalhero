package src.UI.Sidebars.ObjectInfo.Buttons {
	import flash.display.MovieClip;
	import flash.display.SimpleButton;
	import flash.events.Event;
	import flash.events.MouseEvent;
	import src.Global;
	import src.Map.City;
	import src.Map.Map;
	import src.Objects.*;
	import src.Objects.GameObject;
	import src.Objects.Actions.ActionButton;
	import src.Objects.Prototypes.UnitPrototype;
	import src.UI.Cursors.*;
	import src.UI.Dialog.NumberInputDialog;
	import src.UI.Tooltips.TextTooltip;
	import src.Objects.Prototypes.StructurePrototype;

	public class DefaultActionButton extends ActionButton
	{							
		private var textToolTip: TextTooltip;
		private var command:int;
		public function DefaultActionButton(parentObj: GameObject, _commmand: int)
		{					
			super(parentObj, "Default");
			this.command = _commmand;
		
			textToolTip = new TextTooltip("Default Action that you never know what it does!");
			
			addEventListener(MouseEvent.CLICK, onMouseClick);
			addEventListener(MouseEvent.MOUSE_OVER, onMouseOver);
			addEventListener(MouseEvent.MOUSE_MOVE, onMouseOver);
			addEventListener(MouseEvent.MOUSE_OUT, onMouseOut);				
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
			Global.mapComm.Object.defaultAction(parentObj.cityId, parentObj.objectId, command);
				
			/*	var inputDialog: DefaultActionDialog = new DefaultActionDialog();				
				inputDialog.init(Global.map, parentObj as StructureObject, onAcceptDialog, onCloseDialog);
				Global.gameContainer.showDialog(inputDialog);
				*/
			
		}
		
		public override function validateButton():Boolean 
		{
			return true;
		}
		
		/*public function onAcceptDialog(sender: DefaultActionDialog):void
		{
			Global.mapComm.Object.defaultAction(this.parentObj.cityId, this.parentObj.objectId, sender.Command(), sender.Value());
			Global.gameContainer.closeDialog(sender);
		}
		
		public function onCloseDialog(sender: DefaultActionDialog):void
		{
			//Global.gameContainer.closeDialog(sender);
		}		*/
	}
	
}
