package src.FeathersUI.Map {
    import src.Constants;
    import src.FeathersUI.ViewModel;
    import src.Global;
    import src.Map.Camera;
    import src.Map.CityList;
    import src.Map.Region;
    import src.Map.RegionManager;
    import src.Map.UsernameManager;
    import src.Objects.BarbarianTribe;
    import src.Objects.Forest;
    import src.Objects.NewCityPlaceholder;
    import src.Objects.ObjectContainer;
    import src.Objects.SimpleGameObject;
    import src.Objects.SimpleObject;
    import src.Objects.Stronghold.Stronghold;
    import src.Objects.StructureObject;
    import src.Objects.Troop.TroopObject;
    import src.UI.GameJSidebar;
    import src.UI.Sidebars.BarbarianTribeInfo.BarbarianTribeSidebar;
    import src.UI.Sidebars.ForestInfo.ForestInfoSidebar;
    import src.UI.Sidebars.NewCityPlaceholder.NewCityPlaceholderSidebar;
    import src.UI.Sidebars.ObjectInfo.ObjectInfoSidebar;
    import src.UI.Sidebars.StrongholdInfo.StrongholdInfoSidebar;
    import src.UI.Sidebars.TroopInfo.TroopInfoSidebar;
    import src.Util.Util;

    import starling.events.Event;
    import starling.utils.formatString;

    public class MapVM extends ViewModel {
        public static const EVENT_OBJECT_SELECTED: String = "EVENT_OBJECT_SELECTED";

        public static const EVENT_OBJECT_DESELECTED: String = "EVENT_OBJECT_DESELECTED";

        public var regions: RegionManager;

        public var camera: Camera;

        public var pendingRegions: Array;

        public var cities: CityList;

        public var usernames: UsernameManager;

        public var objContainer: ObjectContainer;

        private var timeDelta: int = 0;

        public var selectViewable: Object;

		public var selectedObject: SimpleObject;

        public function MapVM(objContainer: ObjectContainer, camera: Camera) {
            this.camera = camera;
            this.objContainer = objContainer;
            this.pendingRegions = [];
            this.regions = new RegionManager();
            this.cities = new CityList();
            this.usernames = new UsernameManager();
        }

        public function setTimeDelta(timeDelta: int):void
        {
            this.timeDelta = timeDelta;
        }

        public function getServerTime(): int
        {
            var now: Date = new Date();
            return int(now.time / 1000) + timeDelta;
        }

        public function addRegion(id:int, data: Array) : Region
        {
            var newRegion: Region = new Region(id, data, objContainer);

            for (var i:int = pendingRegions.length - 1; i >= 0; i--)
            {
                if (pendingRegions[i] == id)
                {
                    pendingRegions.splice(i, 1);
                }
            }

            regions.add(newRegion);

            if (Constants.debug >= 2) {
                Util.log(formatString("Added region id:{0} to pos:{1},{2}", id, newRegion.x, newRegion.y));
            }

            return newRegion;
        }

        public function selectWhenViewable(groupId: int, objectId: int) : void {
            selectObject(null);

            selectViewable = null;
            for each (var region: Region in regions) {
                var obj: SimpleObject = region.getObject(groupId, objectId);
                if (obj) {
                    selectObject(obj, true, false);
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

                dispatchWith(EVENT_OBJECT_DESELECTED);
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

        public function doSelectedObject(obj: SimpleObject):void
        {
            selectViewable = null;

            if (obj == null)
            {
                selectObject(null);
                return;
            }

            selectedObject = obj;

            obj.setSelected(true);

            dispatchWith(EVENT_OBJECT_SELECTED, obj);
        }

        private function onSelectedObjectDisposed(e: Event): void {
            selectObject(null);
        }

        public function getRegions(requiredRegions: Array): void {
            var outdatedRegions: Array = [];

            //remove any outdated regions from regions we have
            for (var i: int = regions.length - 1; i >= 0; i--) {
                var region: Region = regions.getByIndex(i);

                var found: int = -1;
                for (var a: int = 0; a < requiredRegions.length; a++) {
                    if (region.id == requiredRegions[a]) {
                        found = a;
                        break;
                    }
                }

                if (found >= 0) {
                    if (Constants.debug >= 4) {
                        Util.log("Moved: " + region.id + " " + region.x + "," + region.y);
                    }

                    //remove it from required regions since we already have it
                    requiredRegions.splice(found, 1);
                }
                else {
                    //region is outdated, remove it from buffer
                    outdatedRegions.push(region.id);

                    region.disposeData();
                    regions.removeByIndex(i);

                    if (Constants.debug >= 3) {
                        Util.log("Discarded: " + i);
                    }
                }

                region = null;
            }

            if (Constants.debug >= 3) {
                Util.log("Required before pending removal:" + requiredRegions);
            }

            //remove any pending regions from the required regions list we need
            //and add any regions we are going to be asking the server to the pending regions list
            for (i = requiredRegions.length - 1; i >= 0; i--) {
                if (pendingRegions.indexOf(requiredRegions[i]) > -1) {
                    requiredRegions.splice(i, 1);
                }
                else {
                    pendingRegions.push(requiredRegions[i]);
                }
            }

            //regions that we still need, query server
            if (requiredRegions.length > 0 || outdatedRegions.length > 0) {
                if (Constants.debug >= 3) {
                    Util.log("Required:" + requiredRegions);
                }

                Global.mapComm.Region.getRegion(requiredRegions, outdatedRegions);
            }
        }
    }
}
