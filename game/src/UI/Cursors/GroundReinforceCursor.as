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
	import src.Objects.Actions.StructureUpgradeAction;
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
	import src.UI.PaintBox;
	
	/**
	* ...
	* @author Default
	*/
	public class GroundReinforceCursor extends MovieClip implements IDisposable
	{
		private var map: Map;
		
		private var objX: int;
		private var objY: int;
		
		private var originPoint: Point;
		
		private var cursor: GroundCircle;		
		private var errorMsg: PaintBox;
		
		private var tiles: Array = new Array();
		
		private var troop: Troop;
		private var cityId: int;		
		
		private var highlightedObj: GameObject;
		
		public function GroundReinforceCursor() {
			
		}
				
		public function init(map: Map, troop: Troop, cityId: int):void
		{			
			doubleClickEnabled = true;
						
			this.map = map;
			this.troop = troop;
			this.cityId = cityId;

			map.selectObject(null);
			map.objContainer.resetObjects();									
			
			var size: int = 0;
			
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
			
			setErrorMsg(null);			
			
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
			
			if (gameObj == null || gameObj.objectId != 1)
				return;									
			
			map.mapComm.Troop.troopReinforce(cityId, gameObj.cityId, troop);
			
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
			
			if (gameObj == null || (gameObj as StructureObject) == null || (gameObj as StructureObject).objectId != 1)			
			{				
				msg = 	
					<PaintBox width="-1" color="0xFFFFFF" size="18" bold="true">
						<Row>
							<Column>
								<Text width="-1">Choose town center to defend</Text>
							</Column>
						</Row>
					</PaintBox>;
				
				setErrorMsg(new PaintBox(msg));
			}			
			else
			{
				(gameObj as StructureObject).setHighlighted(true);
				highlightedObj = (gameObj as GameObject);
				
				msg = 	
					<PaintBox width="-1" color="0xFFFFFF" size="18" bold="true">
						<Row>
							<Column>
								<Text width="-1">Double click to defend target</Text>
							</Column>
						</Row>
					</PaintBox>;
				
				setErrorMsg(new PaintBox(msg));				
			}
		}
		
		public function setErrorMsg(errorMsg: PaintBox):void
		{
			if (this.errorMsg != null)			
				stage.removeChild(this.errorMsg);
				
			this.errorMsg = errorMsg;
			
			if (errorMsg != null)
			{
				errorMsg.x = int(Constants.screenW / 2) - int(errorMsg.width / 2);
				errorMsg.y = Constants.screenH - 150 - int(errorMsg.height / 2);
				stage.addChild(errorMsg);					
			}
		}				
	}
	
}