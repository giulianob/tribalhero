﻿
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
	import src.UI.Tooltips.TextTooltip;
	
	public class ForestCampRemoveButton extends ActionButton
	{							
		private var textToolTip: TextTooltip;
		
		public function ForestCampRemoveButton(parentObj: GameObject) 
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
				Global.mapComm.Object.removeForestCamp(parentObj.cityId, parentObj.objectId);
			}
		}
		
		public override function validateButton():Boolean 
		{
			return isEnabled();
		}
	}
	
}