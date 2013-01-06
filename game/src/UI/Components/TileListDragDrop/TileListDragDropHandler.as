package src.UI.Components.TileListDragDrop {
	import fl.controls.TileList;
	import fl.events.ListEvent;
	import flash.display.BitmapData;
	import flash.display.DisplayObject;
	import flash.events.Event;
	import flash.events.MouseEvent;
	import flash.display.Sprite;
	import flash.geom.Point;
	import flash.geom.Rectangle;
	
	/**
	* ...
	* @author Default
	*/
	public class TileListDragDropHandler {				
		
		private var tilelists: Array = new Array();
		
		private var parent: DisplayObject;
		
		private var draggingTilelist: TileList;
		private var draggingItem: Object;
		private var draggingSprite: Sprite;
		
		public function TileListDragDropHandler(parent: DisplayObject) {			
			this.parent = parent;
			
			parent.addEventListener(Event.ADDED_TO_STAGE, onAddedToStage);
			parent.addEventListener(Event.REMOVED_FROM_STAGE, onRemovedFromStage);
			
			if (parent.stage)
				onAddedToStage(new Event(Event.ADDED_TO_STAGE));
		}
		
		public function onAddedToStage(event: Event):void
		{
			parent.stage.addEventListener(MouseEvent.MOUSE_UP, onStageMouseUp);
		}
		
		public function onRemovedFromStage(event: Event):void
		{
			parent.stage.removeEventListener(MouseEvent.MOUSE_UP, onStageMouseUp);					
		}
		
		public function addTileList(tilelist: TileList):void
		{	
			if (!(tilelist as IDraggableTileList))
				throw Error("Cannot convert TileList to IDraggableTileList");
				
			var dditem: TileListDragDropItem = new TileListDragDropItem(tilelist);			
			dditem.addEventListener(TileListDragDropEvent.START_DRAG, onItemStartDrag);
			
			tilelists.push(tilelist);
		}
		
		public function onStageMouseUp(event: MouseEvent):void
		{
			if (!draggingItem) return;

			draggingSprite.stopDrag();
			parent.stage.removeChild(draggingSprite);
			
			for each(var ts: TileList in tilelists)
			{
				if (ts == draggingTilelist)
					continue;
				
				if (ts.hitTestPoint(event.stageX, event.stageY))
				{
					ts.dispatchEvent(new TileListDragDropEvent(TileListDragDropEvent.DRAG_DROPPED, draggingItem, draggingTilelist));					
					
					draggingItem = null;
					draggingTilelist = null;
					return;
				}
			}
			
			draggingItem = null;
			draggingSprite = null;
			draggingTilelist = null;			
		}
		
		
		public function onItemStartDrag(event: Event):void
		{
			var dditem: TileListDragDropItem = event.target as TileListDragDropItem;
			if (!dditem || !parent) return;
			
			draggingSprite = (dditem.tilelist as IDraggableTileList).getDragIcon(dditem.mouseOverItem);
			draggingItem = dditem.mouseOverItem;
			draggingTilelist = dditem.tilelist;			
			parent.stage.addChild(draggingSprite);
			draggingSprite.alpha = 0.80;
			
			draggingSprite.x = parent.stage.mouseX + 5;
			draggingSprite.y = parent.stage.mouseY + 5;
			draggingSprite.startDrag();
			
			dditem.mouseOverItem = null;
		}
	}
	
}