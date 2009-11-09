﻿
package src.UI.Sidebars.TroopInfo.Buttons {
	import flash.display.MovieClip;
	import flash.display.SimpleButton;
	import flash.events.Event;
	import flash.events.MouseEvent;
	import src.Global;
	import src.Map.City;
	import src.Map.Map;
	import src.Objects.Effects.EffectReqManager;
	import src.Objects.Factories.*;
	import src.Objects.GameObject;
	import src.Objects.Actions.ActionButton;
	import src.Objects.SimpleGameObject;
	import src.Objects.TroopObject;
	import src.UI.Cursors.*;
	import src.UI.Dialog.BattleViewer;
	import src.UI.Dialog.Dialog;
	import src.UI.Dialog.TroopInfoDialog;
	import src.UI.Tooltips.BuildTooltip;
	import src.UI.Tooltips.TextTooltip;
	
	public class ViewUnitsButton extends ActionButton
	{		
		private var tooltip: TextTooltip;
		
		public function ViewUnitsButton(parentObj: GameObject)
		{
			super(new ViewUnitsButton_base(), parentObj);
						
			tooltip = new TextTooltip("View Units");
			
			ui.addEventListener(MouseEvent.CLICK, onMouseClick);
			ui.addEventListener(MouseEvent.MOUSE_OVER, onMouseOver);
			ui.addEventListener(MouseEvent.MOUSE_MOVE, onMouseOver);
			ui.addEventListener(MouseEvent.MOUSE_OUT, onMouseOut);
		}
		
		public function onMouseOver(event: MouseEvent):void
		{			
			tooltip.show(this);
		}
		
		public function onMouseOut(event: MouseEvent):void
		{
			tooltip.hide();
		}
		
		public function onMouseClick(event: Event):void
		{
			if (enabled)
			{
				var troopInfo: TroopInfoDialog = new TroopInfoDialog(parentObj as TroopObject);
			
				troopInfo.show();
			}
			
			event.stopImmediatePropagation();
		}		
	}
	
}
