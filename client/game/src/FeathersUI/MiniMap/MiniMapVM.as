package src.FeathersUI.MiniMap {
    import flash.utils.Dictionary;

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
        private var pendingRegions: Dictionary = new Dictionary();
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

            // just for testing
            // it adds random icons at the top right, center, and bottom left of each region
            /*
            var globalX: int = (id % Constants.miniMapRegionRatioW) * Constants.miniMapRegionW;
            var globalY: int = int(id / Constants.miniMapRegionRatioW) * Constants.miniMapRegionH;
            var topLeft: Image = SpriteFactory.getStarlingImage("MINIMAP_SMALL_CIRCLE_SPRITE");
            var topLeftObj: MiniMapRegionObject = newRegion.addRegionObject(0, 1000, 1000, 1, new ScreenPosition(globalX, globalY), {});
            topLeftObj.setIcon(topLeft);

            var center: Image = SpriteFactory.getStarlingImage("COUNT_BUBBLE");
            var centerObj: MiniMapRegionObject = newRegion.addRegionObject(0, 1000, 1000, 1, new ScreenPosition(globalX + Constants.miniMapRegionW/2, globalY + Constants.miniMapRegionH/2), {});
            centerObj.setIcon(center);

            var bottomRight: Image = SpriteFactory.getStarlingImage("ICON_GLOBE_STANDALONE");
            var bottomRightObj: MiniMapRegionObject = newRegion.addRegionObject(0, 1000, 1000, 1, new ScreenPosition(globalX + Constants.miniMapRegionW, globalY + Constants.miniMapRegionH), {});
            bottomRightObj.setIcon(bottomRight);
            */

            if (pendingRegions[id] != null) {
                delete pendingRegions[id];
            }

            regions.add(newRegion);

            return newRegion;
        }

        public function getRegions(requiredRegions: Dictionary): void {
            //remove any outdated regions from regions we have
            for (var i: int = regions.size() - 1; i >= 0; i--) {
                var region: MiniMapRegion = regions.getByIndex(i);

                if (requiredRegions[region.id] == null) {
                    //region is outdated, remove it from buffer
                    region.disposeData();
                    regions.removeByIndex(i);

                    if (Constants.debug >= 3) {
                        Util.log("Discarded minimap region: " + i);
                    }
                } else {
                    //remove it from required regions since we already have it
                    delete requiredRegions[region.id];
                }
            }

            if (Constants.debug >= 3) {
                Util.log("Minimap region required before pending removal:" + requiredRegions);
            }

            var regionsToQuery: Array = [];
            //remove any pending regions from the required regions list we need
            //and add any regions we are going to be asking the server to the pending regions list
            for each (var requiredRegionId:int in requiredRegions) {
                if (pendingRegions[requiredRegionId] == null) {
                    pendingRegions[requiredRegionId] = requiredRegionId;
                    regionsToQuery.push(requiredRegionId);
                }
            }

            //regions that we still need, query server
            if (regionsToQuery.length > 0) {
                if (Constants.debug >= 3) {
                    Util.log("Required minimap regions:" + regionsToQuery);
                }

                Global.mapComm.Region.getMiniMapRegion(regionsToQuery);
            }
        }
    }
}
