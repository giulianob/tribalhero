package src.Map
{	
	import flash.display.DisplayObject;
	import flash.display.MorphShape;
	import flash.display.SpreadMethod;
	import flash.display.Sprite;
	import flash.events.*;
	import flash.filters.GlowFilter;
	import flash.geom.Point;
	import flash.utils.*;	
	import flash.ui.Keyboard;
	import flash.geom.Rectangle;
	import src.GameContainer;
	import src.Global;
	import src.Objects.GameObject;
	import src.Objects.SimpleGameObject;
	import src.Objects.ObjectContainer;
	import src.Objects.StructureObject;
	import src.Objects.TroopObject;
	import src.UI.GameJSidebar;
	import src.UI.Sidebars.ObjectInfo.ObjectInfoSidebar;
	import src.UI.Sidebars.TroopInfo.TroopInfoSidebar;
	
	import src.Constants;
	
	public class Map extends Sprite
	{
		public var regions: RegionList;
		private var mouseDown: Boolean;
		private var mouseLoc: Point;
		private var originPoint: Point = new Point();
		
		public var camera: Camera;
		private var lastQueryTime: int = 0;
		private var pendingRegions: Array;
		
		public var selectViewable: Object;
		public var selectedObject: GameObject;
		public var gameContainer: GameContainer;
		
		private var listenersDefined: Boolean;
		
		private var regionSpace: Sprite;
		private var overlayerSpace: Sprite;
		public var objContainer: ObjectContainer;
		
		public var cities: CityList = new CityList();
		
		public var mapComm: MapComm;
		public var usernames: UsernameManager;
		public var playerId: int;
		
		private var timeDelta: int = 0;
		
		public function Map(camera: Camera)
		{						
			this.camera = camera;
			camera.addEventListener(Camera.ON_MOVE, onMove);
			
			selectedObject = null;					
			
			regionSpace = new Sprite();
			overlayerSpace = new Sprite();
			objContainer = new ObjectContainer(this);
			
			addChild(regionSpace);
			addChild(objContainer);			
			addChild(overlayerSpace);
			
			pendingRegions = new Array();
			regions = new RegionList(this);
			
			addEventListener(Event.ADDED_TO_STAGE, eventAddedToStage);
			addEventListener(Event.REMOVED_FROM_STAGE, eventRemovedFromStage);
			
			listenersDefined = false;
		}
		
		public function init(mapComm: MapComm):void
		{
			this.mapComm = mapComm;
			usernames = new UsernameManager(this);
		}
		
		public function dispose():void
		{
			setGameContainer(null);
			camera.removeEventListener(Camera.ON_MOVE, onMove);
		}
		
		//###################################################################
		//########################### Time ##################################
		//###################################################################
		public function setTimeDelta(timeDelta: int):void
		{
			this.timeDelta = timeDelta;
		}
		
		public function getServerTime(): int
		{
			var now: Date = new Date();
			return int(now.time / 1000) + timeDelta;
		}
		
		//###################################################################
		//########################## Setters ################################
		//###################################################################
		
		public function setGameContainer(gameContainer: GameContainer):void
		{			
			this.gameContainer = gameContainer;
		}
		
		public function disableMouse():void
		{
			if (listenersDefined)
			{		
				stage.removeEventListener(KeyboardEvent.KEY_DOWN, eventKeyDown);
				stage.removeEventListener(MouseEvent.MOUSE_DOWN, eventMouseDown);
				stage.removeEventListener(MouseEvent.MOUSE_MOVE, eventMouseMove);
				stage.removeEventListener(MouseEvent.MOUSE_UP, eventMouseUp);
				
				//for shits and giggles
				//stage.removeEventListener(MouseEvent.MOUSE_WHEEL, eventMouseWheel);
				
				listenersDefined = false;
			}
		}
		
		public function enableMouse():void
		{
			if (!listenersDefined)			
			{
				stage.addEventListener(KeyboardEvent.KEY_DOWN, eventKeyDown);
				stage.addEventListener(MouseEvent.MOUSE_DOWN, eventMouseDown);
				stage.addEventListener(MouseEvent.MOUSE_MOVE, eventMouseMove);
				stage.addEventListener(MouseEvent.MOUSE_UP, eventMouseUp);
				
				//for shits and giggles
				//stage.addEventListener(MouseEvent.MOUSE_WHEEL, eventMouseWheel);
				
				listenersDefined = true;
			}
		}
		
		//###################################################################
		//#################### Region Manipulation ##########################
		//###################################################################
		public function addRegion(id:int, data: Array) : Region
		{
			if (Constants.debug >= 2)
				trace("Adding region: " + id);
				
			var newRegion: Region = new Region(id, data, this);
			
			for (var i:int = pendingRegions.length - 1; i >= 0; i--)
			{
				if (pendingRegions[i] == id)
				{
					pendingRegions.splice(i, 1);
				}
			}
			
			regions.add(newRegion);
			newRegion.moveWithCamera(camera);
			regionSpace.addChild(newRegion);
			
			return newRegion;
		}						
		
		public function parseRegions():void
		{
			if (Constants.debug >= 4)
				trace("on move: " + camera.x + "," + camera.y);
			
			//calculate which regions we need to render
			var requiredRegions: Array = new Array();
			var outdatedRegions: Array = new Array();
			
			const offset:int = 200;
			var requiredId: int = 0;
			
			//middle
			requiredId = MapUtil.getRegionId(camera.x + Constants.screenW/2, camera.y + Constants.screenH/2);
			if (requiredId > -1 && requiredRegions.indexOf(requiredId) == -1)
				requiredRegions.push(requiredId);			
			
			//top-left (up cache)
			requiredId = MapUtil.getRegionId(camera.x, camera.y - offset);
			if (requiredId > -1 && requiredRegions.indexOf(requiredId) == -1)
				requiredRegions.push(requiredId);
			//top-left (left cache)
			requiredId = MapUtil.getRegionId(camera.x - offset, camera.y);
			if (requiredId > -1 && requiredRegions.indexOf(requiredId) == -1)
				requiredRegions.push(requiredId);
			//top-left
			requiredId = MapUtil.getRegionId(camera.x, camera.y);
			if (requiredId > -1 && requiredRegions.indexOf(requiredId) == -1)
				requiredRegions.push(requiredId);
			
			//bottom-left (left)
			requiredId = MapUtil.getRegionId(camera.x - offset, camera.y + Constants.screenH);
			if (requiredId > -1 && requiredRegions.indexOf(requiredId) == -1)
				requiredRegions.push(requiredId);
			//bottom-left (down)
			requiredId = MapUtil.getRegionId(camera.x, camera.y + Constants.screenH + offset);
			if (requiredId > -1 && requiredRegions.indexOf(requiredId) == -1)
				requiredRegions.push(requiredId);
			//bottom-left
			requiredId = MapUtil.getRegionId(camera.x, camera.y + Constants.screenH);
			if (requiredId > -1 && requiredRegions.indexOf(requiredId) == -1)
				requiredRegions.push(requiredId);
				
			//top-right (up)
			requiredId = MapUtil.getRegionId(camera.x + Constants.screenW, camera.y - offset);
			if (requiredId > -1 && requiredRegions.indexOf(requiredId) == -1)
				requiredRegions.push(requiredId);
			//top-right (right)
			requiredId = MapUtil.getRegionId(camera.x + Constants.screenW + offset, camera.y);
			if (requiredId > -1 && requiredRegions.indexOf(requiredId) == -1)
				requiredRegions.push(requiredId);
			//top-right
			requiredId = MapUtil.getRegionId(camera.x + Constants.screenW, camera.y);
			if (requiredId > -1 && requiredRegions.indexOf(requiredId) == -1)
				requiredRegions.push(requiredId);
			
			//bottom-right (down)
			requiredId = MapUtil.getRegionId(camera.x + Constants.screenW, camera.y + Constants.screenH + offset);
			if (requiredId > -1 && requiredRegions.indexOf(requiredId) == -1)
				requiredRegions.push(requiredId);
			//bottom-right (right)
			requiredId = MapUtil.getRegionId(camera.x + Constants.screenW + offset, camera.y + Constants.screenH);
			if (requiredId > -1 && requiredRegions.indexOf(requiredId) == -1)
				requiredRegions.push(requiredId);
			//bottom-right
			requiredId = MapUtil.getRegionId(camera.x + Constants.screenW, camera.y + Constants.screenH);
			if (requiredId > -1 && requiredRegions.indexOf(requiredId) == -1)
				requiredRegions.push(requiredId);
			
			//remove any outdated regions from regions we have		
			for (var i: int = regions.size() - 1; i >= 0; i--)
			{												
				var region: Region = regions.getByIndex(i);
				
				var found: int = -1;
				for (var a:int= 0; a < requiredRegions.length; a++)
				{
					if (region.id == requiredRegions[a])
					{
						found = a;
						break;
					}
				}
				
				if (found >= 0)
				{
					//adjust the position of this region
					region.moveWithCamera(camera);
					
					if (Constants.debug >= 4)
						trace("Moved: " + region.id + " " + region.x + "," + region.y);
						
					//remove it from required regions since we already have it
					requiredRegions.splice(found, 1);
				}
				else
				{
					//region is outdated, remove it from buffer
					outdatedRegions.push(region.id);
					
					region.disposeData();
					regionSpace.removeChild(region);
					regions.removeByIndex(i);
					
					if (Constants.debug >= 4)
						trace("Discarded: " + i);
				}
			}
			
			//remove any pending regions from the required regions list we need
			//and add any regions we are going to be asking the server to the pending regions list
			for (i = requiredRegions.length - 1; i >= 0; i--)
			{
				found = -1;
				
				for (a = 0; a < pendingRegions.length; a++)
				{
					if (pendingRegions[a] == requiredRegions[i])
					{
						found = i;
						break;
					}
				}
				
				if (found >= 0)
				{
					requiredRegions.splice(found, 1);
				}
				else
				{
					pendingRegions.push(requiredRegions[i]);
				}
			}									
			
			//regions that we still need, query server
			if (requiredRegions.length > 0)
			{
				if (Constants.debug >= 3)
					trace("Required:" + requiredRegions);
					
				mapComm.Region.getRegion(requiredRegions, outdatedRegions);
			}
		}
		
		//###################################################################
		//#################### Object Manipulation ##########################
		//###################################################################
		public function selectWhenViewable(cityId: int, objectId: int) : void {
			selectViewable = null;
			for each(var gameObject: GameObject in objContainer.objects) {
				if (SimpleGameObject.compareCityIdAndObjId(gameObject, [cityId, objectId]) == 0) {
					selectObject(gameObject, true, false);
					return;
				}
			}
			
			selectViewable = { 'cityId' : cityId, 'objectId': objectId };			
		}
		
		public function selectObject(obj: GameObject, query: Boolean = true, deselectIfSelected: Boolean = false ):void
		{			
			selectViewable = null;
			
			if (selectedObject != null && obj != null && selectedObject.cityId == obj.cityId && selectedObject.objectId == obj.objectId)
			{
				if (deselectIfSelected)
					obj = null;
				else 
					return;					
			}
			
			gameContainer.setSidebar(null);
		
			if (selectedObject != null)
			{
				selectedObject.hideRadius();
				selectedObject.setSelected(false);
			}
			
			selectedObject = obj;
			
			if (obj != null && query)
			{			
				obj.setSelected(true);
				obj.showRadius();
				
				if (obj is StructureObject)
					mapComm.Object.getStructureInfo(obj as StructureObject);
				else if (obj is TroopObject)
					mapComm.Troop.getTroopInfo(obj as TroopObject);
			}
			else if (obj != null && !query)
			{
				doSelectedObject(obj);
			}
		}
		
		public function doSelectedObject(obj: GameObject):void
		{	
			selectViewable = null;
			
			if (obj == null)
			{				
				selectObject(null);
				return;
			}
			
			selectedObject = obj;
			
			obj.setSelected(true);		
			obj.showRadius();
			
			var sidebar: GameJSidebar;
			
			if (obj is StructureObject)			
				sidebar = new ObjectInfoSidebar(obj as StructureObject);			
			else if (obj is TroopObject)			
				sidebar = new TroopInfoSidebar(obj);			
			
			gameContainer.setSidebar(sidebar);
		}

		//###################################################################
		//##################### Overlayer Commands ##########################
		//###################################################################
		public function addToOverlayer(sprite: Sprite):void
		{
			overlayerSpace.addChild(sprite);
		}
		
		public function removeFromOverlayer(sprite: Sprite):void
		{
			if (overlayerSpace.contains(sprite))
				overlayerSpace.removeChild(sprite);
		}
		
		//###################################################################
		//#################### Mouse/Keyboard Events ########################
		//###################################################################
		public function eventKeyDown(event: KeyboardEvent):void
		{											
			if (event.keyCode == Keyboard.ESCAPE)
			{
				gameContainer.setOverlaySprite(null);
				doSelectedObject(null);				
			}
			
			if (event.keyCode == Keyboard.LEFT)
			{
				camera.MoveLeft(500);
			}
			else if (event.keyCode == Keyboard.RIGHT)
			{
				camera.MoveRight(500);
			}
			else if (event.keyCode == Keyboard.UP)
			{
				camera.MoveUp(500);
			}
			else if (event.keyCode == Keyboard.DOWN)
			{
				camera.MoveDown(500);
			}
			else if (event.keyCode == 187)
			{
				scaleX += 0.1; scaleY += 0.1;
			}
			else if (event.keyCode == 189)
			{
				scaleX -= 0.1; scaleY -= 0.1;
			}
		}
		
		public function eventMouseClick(event: MouseEvent):void
		{
			if (Point.distance(new Point(event.stageX, event.stageY), originPoint) < 4)			
				doSelectedObject(null);			
		}
		
		public function eventMouseWheel(event: MouseEvent):void
		{		
			var delta: int = 0;
			
			if (event.delta < 0)
				delta = -1;
			else if (event.delta > 0)
				delta = 1;
			
			scaleX += delta * 0.1; scaleY += delta * 0.1;
			
			if (scaleX < 0.5) scaleX = 0.5;
			if (scaleY < 0.5) scaleY = 0.5;
			
			if (scaleX > 2.0) scaleX = 2.0;
			if (scaleY > 2.0) scaleY = 2.0;
		}
		
		public function eventMouseDown(event: MouseEvent):void
		{
			originPoint = new Point(event.stageX, event.stageY);
			mouseLoc = new Point(event.stageX, event.stageY);
			mouseDown = true;
			
			if (Constants.debug >= 4)			
				trace("MOUSE DOWN");
		}
		
		public function eventMouseUp(event: MouseEvent):void
		{			
			mouseDown = false;
			
			if (Constants.debug >= 4)			
				trace("MOUSE UP");
		}
		
		public function eventMouseMove(event: MouseEvent):void
		{			
			//debug
			//var pos: Point = MapUtil.getMapCoord(gameContainer.camera.x + Math.max(event.stageX, 0), gameContainer.camera.y + Math.max(event.stageY, 0));			
			//Global.gameContainer.txtCoords.text = pos.x + "," + pos.y;
			
			if (!mouseDown)
			 	return;
			
			var dx: Number = mouseLoc.x - event.stageX;
			var dy: Number = mouseLoc.y - event.stageY;
			
			if (Math.abs(dx) < 2 && Math.abs(dy) < 2) return;
			
			mouseLoc = new Point(event.stageX, event.stageY);
			
			camera.Move(dx, dy);		
		}
		
		public function eventAddedToStage(event: Event):void
		{	
			addEventListener(MouseEvent.CLICK, eventMouseClick);
			enableMouse();
		}
		
		public function eventRemovedFromStage(event: Event):void
		{						
			removeEventListener(MouseEvent.CLICK, eventMouseClick);
			disableMouse();
		}		
		
		public function onMove(event: Event):void
		{
			var pt: Point = MapUtil.getMapCoord(camera.x, camera.y);			
			gameContainer.txtCoords.text = "(" + pt.x + "," + pt.y + ")";			
			
			parseRegions();	
			gameContainer.miniMap.parseRegions();
			objContainer.moveWithCamera(camera.x, camera.y);
			gameContainer.miniMap.objContainer.moveWithCamera(camera.miniMapX, camera.miniMapY);
		}
	}
}