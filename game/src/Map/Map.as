package src.Map
{
    import flash.display.Sprite;
    import flash.events.*;
    import flash.geom.Point;
    import flash.geom.Rectangle;

    import src.Constants;
    import src.Global;
    import src.Objects.BarbarianTribe;
    import src.Objects.Forest;
    import src.Objects.NewCityPlaceholder;
    import src.Objects.ObjectContainer;
    import src.Objects.SimpleGameObject;
    import src.Objects.SimpleObject;
    import src.Objects.Stronghold.Stronghold;
    import src.Objects.StructureObject;
    import src.Objects.Troop.*;
    import src.UI.GameJSidebar;
    import src.UI.Sidebars.BarbarianTribeInfo.BarbarianTribeSidebar;
    import src.UI.Sidebars.ForestInfo.ForestInfoSidebar;
    import src.UI.Sidebars.NewCityPlaceholder.NewCityPlaceholderSidebar;
    import src.UI.Sidebars.ObjectInfo.ObjectInfoSidebar;
    import src.UI.Sidebars.StrongholdInfo.StrongholdInfoSidebar;
    import src.UI.Sidebars.TroopInfo.TroopInfoSidebar;
    import src.Util.Util;

    public class Map extends Sprite
	{
		public var regions: RegionManager;
		private var mouseDown: Boolean;
		private var mouseLoc: Point;
		private var originPoint: Point = new Point();

		private var disabledMapQueries: Boolean;

		public var camera: Camera;

        public var pendingRegions: Array;

		public var selectViewable: Object;
		public var selectedObject: SimpleObject;

		private var listenersDefined: Boolean;

		private var regionSpace: Sprite;
		public var objContainer: ObjectContainer;

		public var cities: CityList = new CityList();

		public var usernames: UsernameManager;

		private var timeDelta: int = 0;

		public var scrollRate: Number = 1;
		
		public function Map() {
            camera = Global.gameContainer.camera;
            camera.addEventListener(Camera.ON_MOVE, onMove);

            selectedObject = null;

            regionSpace = new Sprite();
            objContainer = new ObjectContainer();

            addChild(regionSpace);
            addChild(objContainer);

            pendingRegions = [];
            regions = new RegionManager();

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
					stage.removeEventListener(MouseEvent.MOUSE_DOWN, eventMouseDown);
					stage.removeEventListener(MouseEvent.MOUSE_MOVE, eventMouseMove);
					stage.removeEventListener(MouseEvent.MOUSE_UP, eventMouseUp);
					stage.removeEventListener(Event.MOUSE_LEAVE, eventMouseLeave);

					listenersDefined = false;
				}
			}
			else {
				if (!listenersDefined) {
					stage.addEventListener(MouseEvent.MOUSE_DOWN, eventMouseDown);
					stage.addEventListener(MouseEvent.MOUSE_MOVE, eventMouseMove);
					stage.addEventListener(MouseEvent.MOUSE_UP, eventMouseUp);
					stage.addEventListener(Event.MOUSE_LEAVE, eventMouseLeave);				

					listenersDefined = true;
				}
				stage.focus = Global.map;
			}
		}

		public function disableMapQueries(disabled: Boolean) : void {
			objContainer.disableMouse(disabled);
			disabledMapQueries = disabled;
		}

		//###################################################################
		//#################### Region Manipulation ##########################
		//###################################################################
		public function addRegion(id:int, data: Array) : Region
		{
			if (Constants.debug >= 2)
			Util.log("Adding region: " + id);

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

		public function parseRegions(force: Boolean = false):void {
            if (Constants.debug >= 3) Util.log("On move: " + camera.x + "," + camera.y);

            // Don't parse every single pixel we move
            var lastParseRegionLoc: Point = new Point();
            if (!force && !Math.abs(lastParseRegionLoc.x - camera.x) > 10 && !Math.abs(lastParseRegionLoc.y - camera.y) > 10) return;

            //calculate which regions we need to render
            var requiredRegions: Array = [];
            var outdatedRegions: Array = [];

            // Get list of required regions
            const offset: int = 200;

            var screenRect: Rectangle = new Rectangle(camera.x - offset, camera.y - offset, Constants.screenW * camera.getZoomFactorOverOne() + offset * 2.0, Constants.screenH * camera.getZoomFactorOverOne() + offset * 2.0);
            for (var reqX: int = -1; reqX <= Math.ceil((Constants.screenW * camera.getZoomFactorOverOne()) / Constants.regionW); reqX++) {
                for (var reqY: int = -1; reqY <= Math.ceil((Constants.screenH * camera.getZoomFactorOverOne()) / (Constants.regionH / 2)); reqY++) {
                    var screenPos: ScreenPosition = new ScreenPosition(camera.x + (Constants.regionW * reqX),
                            camera.y + (Constants.regionH / 2 * reqY));
                    var requiredId: int = TileLocator.getRegionId(screenPos);

                    var regionRect: Rectangle = TileLocator.getRegionRect(requiredId);
                    if (!regionRect.containsRect(screenRect) && !screenRect.intersects(regionRect)) continue;

                    if (requiredId > -1 && requiredRegions.indexOf(requiredId) == -1) requiredRegions.push(requiredId);
                }
            }

            //remove any outdated regions from regions we have
            for (var i: int = regions.size() - 1; i >= 0; i--) {
                var region: Region = regions.getByIndex(i);

                var found: int = -1;
                for (var a: int = 0; a < requiredRegions.length; a++) {
                    if (region.id == requiredRegions[a]) {
                        found = a;
                        break;
                    }
                }

                if (found >= 0) {
                    //adjust the position of this region
                    region.moveWithCamera(camera);

                    if (Constants.debug >= 4)
                        Util.log("Moved: " + region.id + " " + region.x + "," + region.y);

                    //remove it from required regions since we already have it
                    requiredRegions.splice(found, 1);
                }
                else {
                    //region is outdated, remove it from buffer
                    outdatedRegions.push(region.id);

                    region.disposeData();
                    regionSpace.removeChild(region);
                    regions.removeByIndex(i);

                    if (Constants.debug >= 3)  Util.log("Discarded: " + i);
                }
            }

            if (Constants.debug >= 3) Util.log("Required before pending removal:" + requiredRegions);

            //remove any pending regions from the required regions list we need
            //and add any regions we are going to be asking the server to the pending regions list
            for (i = requiredRegions.length - 1; i >= 0; i--) {
                if (pendingRegions.indexOf(requiredRegions[i]) > -1) requiredRegions.splice(i, 1);
                else pendingRegions.push(requiredRegions[i]);
            }

            //regions that we still need, query server
            if (requiredRegions.length > 0 || outdatedRegions.length > 0) {
                if (Constants.debug >= 3) Util.log("Required:" + requiredRegions);
                Global.mapComm.Region.getRegion(requiredRegions, outdatedRegions);
            }

            region = null;
        }

		//###################################################################
		//#################### Object Manipulation ##########################
		//###################################################################
		public function selectWhenViewable(groupId: int, objectId: int) : void {
			selectObject(null);

			selectViewable = null;
			for each(var gameObject: SimpleObject in objContainer.objects) {
				if (!(gameObject is SimpleGameObject)) 
					continue;
				
				if (SimpleGameObject.compareGroupIdAndObjId(gameObject as SimpleGameObject, [groupId, objectId]) == 0) {
					selectObject(gameObject, true, false);
					return;
				}
			}

			selectViewable = { 'groupId' : groupId, 'objectId': objectId };
		}
		
		public function requeryIfSelected(obj: SimpleObject):void {
			if (selectedObject !== obj) {
				return;
			}
			
			selectObject(obj);
		}

		public function selectObject(obj: SimpleObject, query: Boolean = true, deselectIfSelected: Boolean = false ):void
		{
			if (selectedObject != null) {
				selectedObject.removeEventListener(SimpleObject.DISPOSED, onSelectedObjectDisposed);
			}
			
			selectViewable = null;
			
			if (obj == null && selectedObject == null) {
				return;
			}
			
			if (obj != null && obj.disposed) {
				obj = null;
			}

			var reselecting: Boolean = false;

			//Check if we are reselecting the currently selected object
			if (selectedObject != null && obj != null && selectedObject == obj)
			{
				//If we are, then deselect it if we have the deselectIfSelected option
				if (deselectIfSelected) 
					obj = null;
				else 
					reselecting = true;
			}

			//If the reselecting bit is on, then we dont want to refresh the whole UI. This just makes a better user experience.
			if (!reselecting) {				
				if (selectedObject != null) {
					selectedObject.setSelected(false);
				}
				
				Global.gameContainer.setSidebar(null);
			}
			
			selectedObject = obj;							
			
			if (obj != null)
			{				
                var gameObj: SimpleGameObject = obj as SimpleGameObject;
                if (gameObj && cities.get(gameObj.groupId)) {
                    Global.gameContainer.selectCity(gameObj.groupId);
                }
                
				selectedObject.addEventListener(SimpleObject.DISPOSED, onSelectedObjectDisposed);
				
				// Decide whether to query for the object info or just go ahead and select it
				if (query) {
					obj.setSelected(true);

					if (obj is StructureObject)
						Global.mapComm.Objects.getStructureInfo(obj as StructureObject);
					else if (obj is TroopObject)
						Global.mapComm.Troop.getTroopInfo(obj as TroopObject);
					else if (obj is Forest)
						Global.mapComm.Objects.getForestInfo(obj as Forest);
					else
						doSelectedObject(obj);
				}
				else
				{
					doSelectedObject(obj);
				}
			}
		}

		private function doSelectedObject(obj: SimpleObject):void
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

			if (obj is StructureObject)
				sidebar = new ObjectInfoSidebar(obj as StructureObject);			
			else if (obj is TroopObject)
				sidebar = new TroopInfoSidebar(obj as TroopObject);			
			else if (obj is Forest)
				sidebar = new ForestInfoSidebar(obj as Forest);
			else if (obj is Stronghold)
				sidebar = new StrongholdInfoSidebar(obj as Stronghold);
			else if (obj is NewCityPlaceholder)
				sidebar = new NewCityPlaceholderSidebar(obj as NewCityPlaceholder);
			else if (obj is BarbarianTribe)
                    sidebar = new BarbarianTribeSidebar(obj as BarbarianTribe);

			Global.gameContainer.setSidebar(sidebar);
		}
		
		private function onSelectedObjectDisposed(e: Event): void {
			selectObject(null);
		}

		//###################################################################
		//#################### Mouse/Keyboard Events ########################
		//###################################################################

		public function eventMouseClick(event: MouseEvent):void
		{
			stage.focus = Global.map;
			
			if (Point.distance(new Point(event.stageX, event.stageY), originPoint) < 4)
				doSelectedObject(null);
		}

		public function eventMouseDown(event: MouseEvent):void
		{
			originPoint = new Point(event.stageX, event.stageY);
			mouseLoc = new Point(event.stageX, event.stageY);
			mouseDown = true;

			if (Constants.debug >= 4)
			Util.log("MOUSE DOWN");
		}

		public function eventMouseUp(event: MouseEvent):void
		{
			mouseDown = false;

			if (Constants.debug >= 4)
				Util.log("MOUSE UP");
		}

		public function eventMouseLeave(event: Event):void
		{
			mouseDown = false;			

			if (Constants.debug >= 4)
				Util.log("MOUSE LEAVE");
		}

		public function eventMouseMove(event: MouseEvent):void
		{                       
			if (!mouseDown) {
                
                if (event.shiftKey) {
                    var screenMouse: Point = TileLocator.getPointWithZoomFactor(event.stageX, event.stageY);
                    var mapPixelPos: ScreenPosition = TileLocator.getActualCoord(camera.x + screenMouse.x, camera.y + screenMouse.y);
                    var mapPos: Point = TileLocator.getMapCoord(mapPixelPos.x, mapPixelPos.y);
                    Global.gameContainer.setLabelCoords(mapPos);
                }
                
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

		public function move(forceParse: Boolean = false) : void {
			var pt: Point = TileLocator.getMapCoord(camera.x + (Constants.screenW * camera.getZoomFactorOverOne()) / 2, camera.y + (Constants.screenH * camera.getZoomFactorOverOne()) / 2);
			Global.gameContainer.setLabelCoords(pt);

			if (!disabledMapQueries) {
				parseRegions(forceParse);
				objContainer.moveWithCamera(camera.x, camera.y);
			}

			Global.gameContainer.miniMap.updatePointers(camera.miniMapCenter);
			Global.gameContainer.miniMap.parseRegions(forceParse);
			Global.gameContainer.miniMap.objContainer.moveWithCamera(camera.miniMapX, camera.miniMapY);
		}

		public function onResize(event: Event = null): void {
			Global.gameContainer.miniMap.redraw();
			move(true);
		}

		public function onMove(event: Event = null) : void
		{
			move();
		}
	}
}

