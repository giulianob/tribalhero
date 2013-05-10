package src.UI.Components.TileListDragDrop {
	import fl.controls.TileList;
	import flash.events.Event;
	
	/**
	* ...
	* @author Default
	*/
	public class TileListDragDropEvent extends Event
	{
		public static const DRAG_DROPPED: String = "DRAG_DROPPED";
		public static const START_DRAG: String = "START_DRAG";
		
		public var item: Object;
		public var dropSource: TileList;
		
		public function TileListDragDropEvent(type: String, item: Object, dropSource: TileList) 
		{
			super(type);
			this.item = item;
			this.dropSource = dropSource;
		}
		
	}
	
}