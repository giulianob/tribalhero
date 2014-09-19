package src.FeathersUI.MiniMap {
    import src.Constants;
    import src.FeathersUI.ViewModel;
    import src.Global;
    import src.Map.Camera;
    import src.Map.MiniMap.MiniMapDrawer;
    import src.Map.MiniMap.MiniMapLegend;
    import src.Map.MiniMap.MiniMapRegion;
    import src.Map.MiniMap.MiniMapRegionList;
    import src.Map.ScreenPosition;
    import src.Objects.ObjectContainer;
    import src.Util.Util;

    public class MiniMapVM extends ViewModel {
        public static const EVENT_NAVIGATE_TO_POINT: String = "EVENT_NAVIGATE_TO_POINT";

        public var regions: MiniMapRegionList;
        public var camera: Camera;
        private var pendingRegions: Array = [];
		public var objContainer: ObjectContainer;

        // TODO: Refactor minimap drawer into view?
        private var mapFilter: MiniMapDrawer = new MiniMapDrawer();
        private var legend: MiniMapLegend = new MiniMapLegend();

        public function MiniMapVM(camera: Camera) {
            this.camera = camera;
            this.objContainer = new ObjectContainer(false, false);
            this.regions = new MiniMapRegionList();

            mapFilter.addOnChangeListener(filterChanged);
            mapFilter.applyLegend(legend);
        }

        public function navigateToPoint(pos: ScreenPosition): void {
            camera.scrollToCenter(pos);
            dispatch(EVENT_NAVIGATE_TO_POINT);
        }

        public function filterChanged(): void {
            for each(var region:MiniMapRegion in regions) {
                region.setFilter(mapFilter);
            }
        }

        public function addMiniMapRegion(id:int) : MiniMapRegion
        {
            if (Constants.debug >= 2)
                Util.log("Adding city region: " + id);

            var newRegion: MiniMapRegion = new MiniMapRegion(id, mapFilter, objContainer);

            for (var i:int = pendingRegions.length - 1; i >= 0; i--)
            {
                if (pendingRegions[i] == id)
                {
                    pendingRegions.splice(i, 1);
                    break;
                }
            }

            regions.add(newRegion);

            return newRegion;
        }

        public function getRegions(requiredRegions: Array): void {
            //remove any outdated regions from regions we have
            for (var i: int = regions.size() - 1; i >= 0; i--) {
                var region: MiniMapRegion = regions.getByIndex(i);

                var found: int = -1;
                for (var a: int = 0; a < requiredRegions.length; a++) {
                    if (region.id == requiredRegions[a]) {
                        found = a;
                        break;
                    }
                }

                if (found >= 0) {
                    //remove it from required regions since we already have it
                    requiredRegions.splice(found, 1);
                }
                else {
                    //region is outdated, remove it from buffer
                    region.disposeData();
                    regions.removeByIndex(i);

                    if (Constants.debug >= 3) {
                        Util.log("Discarded minimap region: " + i);
                    }
                }

                region = null;
            }

            if (Constants.debug >= 3) {
                Util.log("Minimap region required before pending removal:" + requiredRegions);
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
            if (requiredRegions.length > 0) {
                if (Constants.debug >= 3) {
                    Util.log("Required minimap regions:" + requiredRegions);
                }

                Global.mapComm.Region.getMiniMapRegion(requiredRegions);
            }
        }
    }
}
