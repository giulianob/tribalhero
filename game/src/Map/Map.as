package src.Map
{
	import flash.display.Sprite;
	import flash.events.*;
	import flash.geom.Point;
	import flash.utils.*;
	import flash.ui.Keyboard;
	import src.Global;
	import src.Objects.Forest;
	import src.Objects.GameObject;
	import src.Objects.SimpleGameObject;
	import src.Objects.ObjectContainer;
	import src.Objects.StructureObject;
	import src.Objects.Troop.*;
	import src.UI.GameJSidebar;
	import src.UI.Sidebars.ForestInfo.ForestInfoSidebar;
	import src.UI.Sidebars.ObjectInfo.ObjectInfoSidebar;
	import src.UI.Sidebars.TroopInfo.TroopInfoSidebar;

	import src.Constants;

	public class Map extends Sprite
	{
		public var regions: RegionList;
		private var mouseDown: Boolean;
		private var mouseLoc: Point;
		private var originPoint: Point = new Point();

		private var disabledMapQueries: Boolean;

		public var camera: Camera;
		private var lastQueryTime: int = 0;
		private var pendingRegions: Array;

		public var selectViewable: Object;
		public var selectedObject: GameObject;

		private var listenersDefined: Boolean;

		private var regionSpace: Sprite;
		private var overlayerSpace: Sprite;
		public var objContainer: ObjectContainer;

		public var cities: CityList = new CityList();

		public var usernames: UsernameManager;

		private var timeDelta: int = 0;

		public var scrollRate: Number = 1;

		private var pressedKeys:Object = {};

		public function Map()
		{
			camera = Global.gameContainer.camera;
			camera.addEventListener(Camera.ON_MOVE, onMove);

			selectedObject = null;

			regionSpace = new Sprite();
			overlayerSpace = new Sprite();
			objContainer = new ObjectContainer();

			addChild(regionSpace);
			addChild(objContainer);
			addChild(overlayerSpace);

			pendingRegions = new Array();
			regions = new RegionList();

			usernames = new UsernameManager();

			addEventListener(Event.ADDED_TO_STAGE, eventAddedToStage);
			addEventListener(Event.REMOVED_FROM_STAGE, eventRemovedFromStage);

			listenersDefined = false;
		}

		public function dispose():void
		{
			camera.removeEventListener(Camera.ON_MOVE, onMove);
			disableMouse(true);
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

		public function disableMouse(disable: Boolean):void
		{
			if (disable) {
				if (listenersDefined)
				{
					stage.removeEventListener(KeyboardEvent.KEY_DOWN, eventKeyDown);
					stage.removeEventListener(KeyboardEvent.KEY_UP, eventKeyUp);
					stage.removeEventListener(MouseEvent.MOUSE_DOWN, eventMouseDown);
					stage.removeEventListener(MouseEvent.MOUSE_MOVE, eventMouseMove);
					stage.removeEventListener(MouseEvent.MOUSE_UP, eventMouseUp);
					stage.removeEventListener(Event.MOUSE_LEAVE, eventMouseLeave);

					listenersDefined = false;
				}
			}
			else {
				if (!listenersDefined) {
					stage.addEventListener(KeyboardEvent.KEY_UP, eventKeyUp);
					stage.addEventListener(KeyboardEvent.KEY_DOWN, eventKeyDown);
					stage.addEventListener(MouseEvent.MOUSE_DOWN, eventMouseDown);
					stage.addEventListener(MouseEvent.MOUSE_MOVE, eventMouseMove);
					stage.addEventListener(MouseEvent.MOUSE_UP, eventMouseUp);
					stage.addEventListener(Event.MOUSE_LEAVE, eventMouseLeave);

					listenersDefined = true;
				}
			}
		}

		public function disableMapQueries(disabled: Boolean) : void {
			objContainer.disableMouse(disabled);
			disabledMapQueries = disabled;

			/* Disabled due to crazy memory usage by blur filter
			if (!disabled) {
			filters = [];
			} else {
			filters = [new BlurFilter(10, 10)];
			}
			*/
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
				if (pendingRegions.indexOf(requiredRegions[i]) > -1)
				{
					requiredRegions.splice(i, 1);
				}
				else
				{
					pendingRegions.push(requiredRegions[i]);
				}
			}

			//regions that we still need, query server
			if (requiredRegions.length > 0 || outdatedRegions.length > 0)
			{
				if (Constants.debug >= 3)
				trace("Required:" + requiredRegions);

				Global.mapComm.Region.getRegion(requiredRegions, outdatedRegions);
			}

			region = null;
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

			var reselecting: Boolean = false;

			//Check if we are reselecting the currently selected object
			if (selectedObject != null && obj != null && selectedObject.cityId == obj.cityId && selectedObject.objectId == obj.objectId)
			{
				//If we are, then deselect it if we have the deselectIfSelected option
				if (deselectIfSelected)
				obj = null;
				else
				reselecting = true;
			}

			//If the reselecting bit is on, then we dont want to refresh the whole UI. This just makes a better user experience.
			if (!reselecting) {
				Global.gameContainer.setSidebar(null);

				if (selectedObject != null)
				selectedObject.setSelected(false);

				selectedObject = obj;
			}

			if (obj != null)
			{
				if (query) {
					obj.setSelected(true);

					if (obj is StructureObject) {
						Global.mapComm.Object.getStructureInfo(obj as StructureObject);
					}
					else if (obj is TroopObject) {
						Global.mapComm.Troop.getTroopInfo(obj as TroopObject);
					}
					else if (obj is Forest) {
						Global.mapComm.Object.getForestInfo(obj as Forest);
					}
				}
				else {
					doSelectedObject(obj);
					return;
				}
			}
		}

		private function doSelectedObject(obj: GameObject):void
		{
			selectViewable = null;

			if (obj == null)
			{
				selectObject(null);
				return;
			}

			selectedObject = obj;

			obj.setSelected(true);

			var sidebar: GameJSidebar;

			if (obj is StructureObject) {
				sidebar = new ObjectInfoSidebar(obj as StructureObject);
			}
			else if (obj is TroopObject) {
				sidebar = new TroopInfoSidebar(obj);
			}
			else if (obj is Forest) {
				sidebar = new ForestInfoSidebar(obj as Forest);
			}

			Global.gameContainer.setSidebar(sidebar);
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

		public function eventKeyUp(event: KeyboardEvent):void
		{
			// clear key press
			delete pressedKeys[event.keyCode];
		}

		public function eventKeyDown(event: KeyboardEvent):void
		{
			// Key down

			//end key down

			// Key Press Handler
			if(pressedKeys[event.keyCode]) return;
			pressedKeys[event.keyCode] = 1;

			// Key press
			if (event.keyCode == Keyboard.ESCAPE) doSelectedObject(null);

			camera.beginMove();
			if (event.keyCode == Keyboard.LEFT) camera.MoveLeft(500);
			if (event.keyCode == Keyboard.RIGHT) camera.MoveRight(500);
			if (event.keyCode == Keyboard.UP) camera.MoveUp(500);
			if (event.keyCode == Keyboard.DOWN) camera.MoveDown(500);
			camera.endMove();
			//end key press
		}

		public function eventMouseClick(event: MouseEvent):void
		{
			if (Point.distance(new Point(event.stageX, event.stageY), originPoint) < 4)
			doSelectedObject(null);
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

		public function eventMouseLeave(event: Event):void
		{
			mouseDown = false;

			if (Constants.debug >= 4)
			trace("MOUSE LEAVE");
		}

		public function eventMouseMove(event: MouseEvent):void
		{
			if (!mouseDown) {
				return;
			}

			var dx: Number = (mouseLoc.x - event.stageX) * scrollRate;
			var dy: Number = (mouseLoc.y - event.stageY) * scrollRate;

			if (Math.abs(dx) < 1 && Math.abs(dy) < 1) return;

			mouseLoc = new Point(event.stageX, event.stageY);

			camera.Move(dx, dy);
		}

		public function eventAddedToStage(event: Event):void
		{
			addEventListener(MouseEvent.CLICK, eventMouseClick);
			disableMouse(false);
		}

		public function eventRemovedFromStage(event: Event):void
		{
			removeEventListener(MouseEvent.CLICK, eventMouseClick);
			disableMouse(true);
		}

		public function onMove(event: Event = null):void
		{
			var pt: Point = MapUtil.getMapCoord(camera.x, camera.y);
			Global.gameContainer.minimapTools.txtCoords.text = "(" + pt.x + "," + pt.y + ")";

			if (!disabledMapQueries) {
				parseRegions();
				objContainer.moveWithCamera(camera.x, camera.y);
			}

			Global.gameContainer.miniMap.parseRegions();
			Global.gameContainer.miniMap.objContainer.moveWithCamera(camera.miniMapX, camera.miniMapY);
		}
	}
}

