﻿
package src.UI.Sidebars.ObjectInfo.Buttons {
	import flash.events.Event;
	import flash.events.MouseEvent;
	import src.Objects.Factories.*;
	import src.Objects.GameObject;
	import src.Objects.Actions.ActionButton;
	import src.UI.Cursors.*;
	import src.UI.Dialog.BattleViewer;
	import src.UI.Tooltips.TextTooltip;

	public class ViewBattleButton extends ActionButton
	{
		private var tooltip: TextTooltip;

		public function ViewBattleButton(parentObj: GameObject)
		{
			super(new ViewBattleButton_base(), parentObj);

			tooltip = new TextTooltip("View Battle");

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
				var battleViewer: BattleViewer = new BattleViewer(parentObj.battleCityId);
				battleViewer.show();
			}

			event.stopImmediatePropagation();
		}
	}

}

