package src.Objects {

	import flash.display.MovieClip;
	import flash.events.Event;
	import flash.events.MouseEvent;
	import flash.filters.GlowFilter;
	import flash.geom.Point;
	import src.Map.Camera;
	import src.Constants;
	import src.Map.Map;
	
	public class GameObject extends SimpleGameObject implements IScrollableObject
	{
		private var onSelect: Function;
		private var selected: Boolean;
		
		private var ignoreClick: Boolean;
		private var originClick: Point;
		
		public function GameObject()
		{			
			ignoreClick = false;
			originClick = new Point(0, 0);			
		}		
		
		public function setOnSelect(callback: Function):void
		{
			onSelect = callback;
		}
		
		public function setSelected(bool: Boolean = false):void
		{					
			if (bool == false)
			{
				filters = new Array();
			}
			else
			{			
				var tFilters:Array = new Array();
				tFilters.push(new GlowFilter(0xFFFFFF));
				filters = tFilters;
			}
			
			selected = bool;
		}
		
		public function setHighlighted(bool: Boolean = false):void
		{
			if (selected) 
				return;
				
			if (bool == false)
			{
				filters = new Array();
			}
			else
			{
				var tFilters:Array = new Array();
				tFilters.push(new GlowFilter(0xFF3300));
				filters = tFilters;
			}
		}	
	}	
}