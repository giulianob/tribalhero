package src.UI.Cursors {
	import flash.display.Bitmap;
	import flash.display.BitmapData;
	import flash.display.MovieClip;
	import flash.display.Sprite;
	import flash.events.Event;
	import flash.events.MouseEvent;
	import flash.events.KeyboardEvent;
	import flash.filters.BlurFilter;
	import flash.filters.ColorMatrixFilter;
	import flash.geom.ColorTransform;
	import flash.geom.Point;
	import src.Constants;
	import src.Map.City;
	import src.Objects.Actions.StructureUpgradeAction;
	import src.Objects.Effects.Formula;
	import src.Objects.GameObject;
	import src.Objects.ObjectContainer;
	import src.Objects.SimpleGameObject;
	import src.Objects.SimpleObject;
	import flash.ui.Mouse;
	import src.Map.Map;
	import src.Map.MapUtil;
	import src.Objects.IDisposable;	
	import src.Objects.StructureObject;
	import src.Objects.Troop;
	import src.UI.Components.GroundCircle;
	import src.UI.Tooltips.Tooltip;
	import src.Util.Util;
	
	/**
	* ...
	* @author Default
	*/
	public class GroundAttackCursor extends MovieClip implements IDisposable
	{
		private var map: Map;
		
		private var objX: int;
		private var objY: int;
		
		private var originPoint: Point;
		
		private var cursor: GroundCircle;		
		
		private var tiles: Array = new Array();
		
		private var troop: Troop;
		private var city: City;
		private var mode: int;
		
		private var highlightedObj: GameObject;
		
		private var tooltip: Tooltip = new Tooltip();
		
		public function GroundAttackCursor() {
			
		}
				
		public function init(map: Map, troop: Troop, mode: int, cityId: int):void
		{			
			doubleClickEnabled = true;
						
			this.map = map;
			this.troop = troop;
			this.city = map.cities.get(cityId);
			this.mode = mode;
			
			map.selectObject(null);
			map.objContainer.resetObjects();									
			
			var size: int = Math.max(1, int(troop.getIndividualUnitCount() / 25));
			
			cursor = new GroundCircle(size);			
			cursor.alpha = 0.6;			
		
			map.objContainer.addObject(cursor, ObjectContainer.LOWER);
			
			addEventListener(Event.ADDED_TO_STAGE, onAddedToStage);
			addEventListener(MouseEvent.DOUBLE_CLICK, onMouseDoubleClick);
			addEventListener(MouseEvent.CLICK, onMouseStop, true);
			addEventListener(MouseEvent.MOUSE_MOVE, onMouseMove);
			addEventListener(MouseEvent.MOUSE_OVER, onMouseStop);
			addEventListener(MouseEvent.MOUSE_DOWN, onMouseDown);
			
			map.gameContainer.setOverlaySprite(this);
		}
		
		public function onAddedToStage(e: Event):void
		{
			moveTo(stage.mouseX, stage.mouseY);
		}
				
		public function dispose():void
		{			
			if (cursor != null)
			{												
				map.objContainer.removeObject(cursor, ObjectContainer.LOWER);							
				cursor.dispose();
			}
			
			map.gameContainer.message.hide();
			
			if (highlightedObj)
			{
				highlightedObj.setHighlighted(false);
				highlightedObj = null;
			}
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
						
			var gameObj: SimpleGameObject = map.regions.getObjectAt(objX, objY);
			
			if (gameObj == null)
				return;												
			
			map.mapComm.Troop.troopAttack(city.id, gameObj.cityId, gameObj.objectId, mode, troop);
			
			map.gameContainer.setOverlaySprite(null);
			map.gameContainer.setSidebar(null);
			map.selectObject(null);			
		}		
		
		public function onMouseDown(event: MouseEvent):void
		{
			originPoint = new Point(event.stageX, event.stageY);
		}
		
		public function onMouseMove(event: MouseEvent):void
		{						
			if (event.buttonDown)
				return;
		
			moveTo(event.stageX, event.stageY);
		}
		
		public function moveTo(x: int, y: int):void
		{
			var pos: Point = MapUtil.getActualCoord(map.gameContainer.camera.x + Math.max(x, 0), map.gameContainer.camera.y + Math.max(y, 0));			
			
			if (pos.x != objX || pos.y != objY)
			{	
				map.objContainer.removeObject(cursor, ObjectContainer.LOWER);
				
				objX = pos.x;
				objY = pos.y;		
				
				cursor.setX(objX);
				cursor.setY(objY);
				
				cursor.moveWithCamera(map.gameContainer.camera);									
				
				map.objContainer.addObject(cursor, ObjectContainer.LOWER);
				
				validate();
			}			
		}
		
		public function validate():void
		{
			if (highlightedObj)
			{
				highlightedObj.setHighlighted(false);
				highlightedObj = null;
			}
			
			var msg: XML;			
			
			var gameObj: SimpleGameObject = map.regions.getObjectAt(objX, objY);
			
			if (gameObj == null || (gameObj as StructureObject) == null)			
			{				
				map.gameContainer.message.showMessage("Choose target to attack");
			}			
			else
			{
				var structObj: StructureObject = gameObj as StructureObject;
				structObj.setHighlighted(true);
				highlightedObj = (gameObj as GameObject);
				
				var targetMapDistance: Point = MapUtil.getMapCoord(structObj.getX(), structObj.getY());
				var distance: int = city.MainBuilding.distance(targetMapDistance.x, targetMapDistance.y);
				var timeAwayInSeconds: int = Math.max(1, Formula.moveTime(troop.getSpeed()) * Constants.secondsPerUnit * distance);
				
				map.gameContainer.message.showMessage("About " + Util.niceTime(timeAwayInSeconds) + " away. Double click to attack.");			
			}
		}		
	}
	
}