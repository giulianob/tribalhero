
package src.UI.Sidebars.ObjectInfo.Buttons {
    import flash.events.Event;
    import flash.events.MouseEvent;

    import src.Global;
    import src.Objects.*;
    import src.Objects.Actions.ActionButton;
    import src.UI.Tooltips.TextTooltip;

    public class ForestCampRemoveButton extends ActionButton
	{							
		private var textToolTip: TextTooltip;
		
		public function ForestCampRemoveButton(parentObj: SimpleGameObject) 
		{			
			super(parentObj, "Demolish Camp");
			
			textToolTip = new TextTooltip("Demolish Camp");
			
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
			if (isEnabled())
			{
				Global.mapComm.Objects.removeForestCamp(parentObj.groupId, parentObj.objectId);
			}
		}
	}
	
}