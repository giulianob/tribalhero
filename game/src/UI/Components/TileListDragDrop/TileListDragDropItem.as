package src.UI.Components.TileListDragDrop {
	import fl.controls.TileList;
	import fl.events.ListEvent;
	import flash.events.Event;
	import flash.events.EventDispatcher;
	import flash.events.MouseEvent;
	import flash.display.Sprite;
	
	/**
	* ...
	* @author Default
	*/
	public class TileListDragDropItem extends EventDispatcher {			
		
		public var tilelist: TileList;
		public var mouseOverItem: Object;
		private var mouseDown: Boolean = false;
		
		public function TileListDragDropItem(tilelist: TileList) {
			this.tilelist = tilelist;
			
			tilelist.addEventListener(ListEvent.ITEM_ROLL_OVER, onRollOver);
			tilelist.addEventListener(ListEvent.ITEM_ROLL_OUT, onRollOut);
			tilelist.addEventListener(MouseEvent.MOUSE_DOWN, onMouseDown);
			tilelist.addEventListener(MouseEvent.MOUSE_UP, onMouseUp);
		}
		
		public function onRollOver(event: ListEvent):void
		{			
			mouseOverItem = event.item;
		}
		
		public function onRollOut(event: ListEvent):void
		{
			if (mouseDown && mouseOverItem)
			{
				mouseDown = false;
				dispatchEvent(new Event(TileListDragDropEvent.START_DRAG));
			}
			
			mouseOverItem = null;
		}
		
		public function onMouseDown(event: MouseEvent):void
		{
			mouseDown = true;
		}
		
		public function onMouseUp(event: MouseEvent):void
		{
			mouseDown = false;
		}
	}
	
}