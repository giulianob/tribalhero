
package src.UI.Sidebars.ObjectInfo.Buttons {
	import flash.events.Event;
	import flash.events.MouseEvent;
	import flash.geom.Point;
	import src.Global;
	import src.Map.MapUtil;
	import src.Objects.Factories.*;
	import src.Objects.GameObject;
	import src.Objects.Actions.ActionButton;
	import src.Objects.States.MovingState;
	import src.UI.Cursors.*;
	import src.UI.Tooltips.TextTooltip;

	public class ViewDestinationButton extends ActionButton
	{
		private var tooltip: TextTooltip;

		private var mapDestinationPos: Point;

		public function ViewDestinationButton(parentObj: GameObject, mapDestinationPos: Point)
		{
			super(parentObj, "View Destination");

			tooltip = new TextTooltip("View Destination");

			this.mapDestinationPos = mapDestinationPos;

			addEventListener(MouseEvent.CLICK, onMouseClick);
			addEventListener(MouseEvent.MOUSE_OVER, onMouseOver);
			addEventListener(MouseEvent.MOUSE_MOVE, onMouseOver);
			addEventListener(MouseEvent.MOUSE_OUT, onMouseOut);
		}

		// Override disable since this button can always be clicked
		override public function disable():void
		{
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
			if (isEnabled())
			{
				var pt: Point = MapUtil.getScreenCoord(mapDestinationPos.x, mapDestinationPos.y);
				Global.map.camera.ScrollToCenter(pt.x, pt.y);
			}

			event.stopImmediatePropagation();
		}
	}

}

