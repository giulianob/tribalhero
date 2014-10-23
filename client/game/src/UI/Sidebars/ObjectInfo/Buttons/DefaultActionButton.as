package src.UI.Sidebars.ObjectInfo.Buttons {
    import flash.events.Event;
    import flash.events.MouseEvent;

    import src.Global;
    import src.Objects.*;
    import src.FeathersUI.Controls.ActionButton;
    import src.UI.Tooltips.TextTooltip;

    public class DefaultActionButton extends ActionButton
	{							
		private var textToolTip: TextTooltip;
		private var command:int;
		public function DefaultActionButton(parentObj: SimpleGameObject, _commmand: int)
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
			Global.mapComm.Objects.defaultAction(parentObj.groupId, parentObj.objectId, command);			
		}
		
		public override function validateButton():Boolean 
		{
			return true;
		}
	}
	
}
