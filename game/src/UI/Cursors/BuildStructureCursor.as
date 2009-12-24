
package src.UI.Cursors {	
	import flash.filters.GlowFilter;
	import flash.geom.Point;
	import flash.ui.Keyboard;
	import flash.ui.Mouse;
	import src.Map.Camera;
	import src.Map.City;
	import src.Map.Map;
	import src.Map.MapUtil;
	import src.Constants;
	import flash.display.DisplayObject;	
	import flash.display.MovieClip;
	import flash.events.Event;
	import flash.events.MouseEvent;
	import src.Objects.Factories.StructureFactory;
	import src.Objects.GameObject;
	import src.Objects.IScrollableObject;	
	import src.Objects.IDisposable;
	import src.Objects.ObjectContainer;
	import src.Objects.Prototypes.StructurePrototype;
	import src.Objects.SimpleObject;
	import src.UI.Components.GroundCircle;
	import src.UI.Sidebars.CursorCancel.CursorCancelSidebar;
	import src.UI.SmartMovieClip;
	
	public class BuildStructureCursor extends MovieClip implements IDisposable
	{		
		private var map: Map;
		private var objX: int;
		private var objY: int;
		
		private var originPoint: Point;
		
		private var cursor: SimpleObject;
		private var rangeCursor: GroundCircle;
		private var structPrototype: StructurePrototype;		
		private var parentObj: GameObject;
		private var type: int;
		private var level: int;
			
		public function BuildStructureCursor() { }
		
		public function init(map: Map, type: int, level: int, parentObject: GameObject):void
		{			
			doubleClickEnabled = true;
			
			this.parentObj = parentObject;
			
			this.type = type;
			this.level = level;
			
			this.map = map;
			map.gameContainer.setOverlaySprite(this);
			map.selectObject(null);
			
			structPrototype = StructureFactory.getPrototype(type, level);
			cursor = StructureFactory.getSimpleObject(type, level);
			 
			if (StructureFactory.getSimpleObject(type, level) == null)
			{				
				trace("Missing building cursor " + type);
				return;
			}
			
			cursor.alpha = 0.7;						
			
			rangeCursor = new GroundCircle(structPrototype.range);
			rangeCursor.alpha = 0.6;				
			
			var sidebar: CursorCancelSidebar = new CursorCancelSidebar(parentObj);
			map.gameContainer.setSidebar(sidebar);
			
			addEventListener(MouseEvent.DOUBLE_CLICK, onMouseDoubleClick);
			addEventListener(MouseEvent.CLICK, onMouseStop, true);
			addEventListener(MouseEvent.MOUSE_MOVE, onMouseMove);
			addEventListener(MouseEvent.MOUSE_OVER, onMouseStop);
			addEventListener(MouseEvent.MOUSE_DOWN, onMouseDown);
		}
		
		public function dispose():void
		{			
			map.gameContainer.message.hide();
			
			if (cursor != null)
			{								
				if (cursor.stage != null) map.objContainer.removeObject(cursor);			
				if (rangeCursor.stage != null) map.objContainer.removeObject(rangeCursor, ObjectContainer.LOWER);
			}
		}	
		
		private function showCursors() : void {
			if (cursor) cursor.visible = true;
			if (rangeCursor) rangeCursor.visible = true;
		}
		
		private function hideCursors() : void {
			if (cursor) cursor.visible = false;
			if (rangeCursor) rangeCursor.visible = false;			
		}
		
		public function onMouseStop(event: MouseEvent):void
		{
			event.stopImmediatePropagation();				
		}
		
		public function onMouseDoubleClick(event: MouseEvent):void
		{
			if (Point.distance(new Point(event.stageX, event.stageY), originPoint) > 4)
				return;
				
			event.stopImmediatePropagation();	
						
			var pos: Point = MapUtil.getMapCoord(objX, objY);
			map.mapComm.Object.buildStructure(parentObj.cityId, parentObj.objectId, type, pos.x, pos.y);
			
			map.gameContainer.setOverlaySprite(null);
			map.gameContainer.setSidebar(null);
			map.selectObject(parentObj, false);
		}		
		
		public function onMouseDown(event: MouseEvent):void
		{
			originPoint = new Point(event.stageX, event.stageY);
		}
		
		public function onMouseMove(event: MouseEvent) : void
		{						
			if (event.buttonDown)
				return;
			
			var pos: Point = MapUtil.getActualCoord(map.gameContainer.camera.x + Math.max(event.stageX, 0), map.gameContainer.camera.y + Math.max(event.stageY, 0));			
			
			if (pos.x != objX || pos.y != objY)
			{					
				objX = pos.x;
				objY = pos.y;	
				
				//Range cursor
				if (rangeCursor.stage != null) map.objContainer.removeObject(rangeCursor, ObjectContainer.LOWER);				
				rangeCursor.setX(objX); rangeCursor.setY(objY);				
				rangeCursor.moveWithCamera(map.gameContainer.camera);				
				map.objContainer.addObject(rangeCursor, ObjectContainer.LOWER);
				
				//Object cursor
				if (cursor.stage != null) map.objContainer.removeObject(cursor);																		
				cursor.setX(objX); cursor.setY(objY);				
				cursor.moveWithCamera(map.gameContainer.camera);																	
				map.objContainer.addObject(cursor);				
				validateBuilding();
			}
		}
		
		public function validateBuilding():void
		{				
			var msg: XML;			
			
			var city: City = map.cities.get(parentObj.cityId);
			var mapObjPos: Point = MapUtil.getMapCoord(objX, objY);
			
			if (map.regions.getObjectAt(objX, objY) != null)
			{
					hideCursors();
					
					map.gameContainer.message.showMessage("Can not build on top of another structure");					
					return;
			}
			else if (city != null && MapUtil.distance(city.MainBuilding.x, city.MainBuilding.y, mapObjPos.x, mapObjPos.y) >= city.radius) {
					hideCursors();				
					map.gameContainer.message.showMessage("This structure must be built within the city walls");
					return;
			}
			else
			{
				showCursors();
				map.gameContainer.message.hide();
			}
			
			if (structPrototype != null)
			{	
				var filters: Array = new Array();
				
				if (structPrototype.validateLayout(map, map.cities.get(parentObj.cityId), objX, objY))
				{				
					filters.push(new GlowFilter(0x00FF00));									
				}
				else
				{					
					filters.push(new GlowFilter(0xFF0000));				
					
					map.gameContainer.message.showMessage("Some of the building requirements have not been met");
				}
				
				showCursors();
			}
		}
	}
}