package src.FeathersUI.GameScreen {
    import feathers.controls.Button;
    import feathers.controls.Label;
    import feathers.controls.LayoutGroup;
    import feathers.controls.PickerList;
    import feathers.controls.Screen;
    import feathers.data.ListCollection;
    import feathers.layout.AnchorLayout;
    import feathers.layout.AnchorLayoutData;
    import feathers.layout.HorizontalLayout;
    import feathers.layout.VerticalLayout;

    import src.FeathersUI.Controls.HoverTooltip;
    import src.FeathersUI.Map.MapView;
    import src.FeathersUI.MiniMap.MiniMapVM;
    import src.FeathersUI.MiniMap.MiniMapView;
    import src.FeathersUI.MiniMapDrawer.MiniMapDrawer;
    import src.FeathersUI.Themes.TribalHeroTheme;
    import src.Map.Camera;
    import src.Map.City;
    import src.Map.Position;
    import src.Objects.Factories.SpriteFactory;
    import src.Util.BinaryList.BinaryListEvent;

    import starling.display.DisplayObject;
    import starling.events.Event;
    import starling.utils.formatString;

    public class GameScreenDesktopView extends Screen {
        private var vm: GameScreenVM;
        private var map: MapView;
        private var minimap: MiniMapView;
        private var btnMinimapZoom: Button;
        private var btnFind: Button;
        private var btnSendFeedback: Button;
        private var btnMute: Button;
        private var lblCoords: Label;
        private var isMinimapZoomed: Boolean;
        private var miniMapFilters: LayoutGroup;
        private var miniMapDrawer: MiniMapDrawer;
        private var miniMapContainerLayoutDataUnzoomed: AnchorLayoutData;
        private var miniMapContainerLayoutDataZoomed: AnchorLayoutData;
        private var minimapContainer: LayoutGroup;
        private var miniMapVm: MiniMapVM;
        private var selectedCityButton: Button;
        private var cityPickerList: PickerList;
        private var cityPickerListData: ListCollection;

        public function GameScreenDesktopView(vm: GameScreenVM, mapView: MapView, miniMapVm: MiniMapVM, miniMap: MiniMapView) {
            this.vm = vm;
            this.map = mapView;
            this.miniMapVm = miniMapVm;
            this.minimap = miniMap;
            this.miniMapDrawer = miniMapVm.mapFilter;
        }

        override protected function initialize():void
        {
            super.initialize();

            map.camera.addEventListener(Camera.ON_MOVE, updateCoordinates);
            miniMapVm.addEventListener(MiniMapVM.EVENT_NAVIGATE_TO_POINT, onMiniMapNavigateToPoint);

            this.layout = new AnchorLayout();

            minimap.layoutData = new AnchorLayoutData(NaN, NaN, 0, 0);
            minimap.scrollRate = 0.25;

            var menuContainer: LayoutGroup = new LayoutGroup();
            {
                menuContainer.layoutData = new AnchorLayoutData(TribalHeroTheme.PADDING_DEFAULT, NaN, NaN, TribalHeroTheme.PADDING_DEFAULT);

                menuContainer.layout = new HorizontalLayout();

                cityPickerList = new PickerList();
                cityPickerList.labelFunction = function(item: Object): String { return ""; };
                cityPickerList.maxWidth = 24;
                cityPickerList.dataProvider = initializeCityPickerListData();
                cityPickerList.selectedIndex = getCityPickerIndex(vm.selectedCity);
                cityPickerList.addEventListener(Event.CHANGE, onCityPickerListChange);

                selectedCityButton = new Button();
                selectedCityButton.label = vm.selectedCity != null ? vm.selectedCity.name : "";

                menuContainer.addChild(selectedCityButton);
                menuContainer.addChild(cityPickerList);
            }

            minimapContainer = new LayoutGroup();
            {
                var minimapTools: LayoutGroup = new LayoutGroup();
                {
                    var minimapToolsLayoutData: AnchorLayoutData = new AnchorLayoutData(NaN, NaN, TribalHeroTheme.PADDING_DEFAULT);
                    minimapToolsLayoutData.bottomAnchorDisplayObject = minimap;
                    minimapTools.layoutData = minimapToolsLayoutData;

                    var minimapToolsLayout: HorizontalLayout = new HorizontalLayout();
                    minimapToolsLayout.verticalAlign = HorizontalLayout.VERTICAL_ALIGN_MIDDLE;
                    minimapToolsLayout.gap = 10;
                    minimapTools.layout = minimapToolsLayout;

                    btnMinimapZoom = new Button();
                    btnMinimapZoom.styleProvider = null;
                    btnMinimapZoom.defaultIcon = SpriteFactory.getStarlingImage("ICON_MAP");
                    btnMinimapZoom.addEventListener(Event.TRIGGERED, onTriggerMinimapZoom);
                    new HoverTooltip("World View", btnMinimapZoom).bind();

                    btnFind = new Button();
                    btnFind.styleProvider = null;
                    btnFind.defaultIcon = SpriteFactory.getStarlingImage("ICON_GLOBE_STANDALONE");
                    new HoverTooltip("Find...", btnFind).bind();

                    btnSendFeedback = new Button();
                    btnSendFeedback.styleProvider = null;
                    btnSendFeedback.defaultIcon = SpriteFactory.getStarlingImage("ICON_BOOK_SMALL");
                    new HoverTooltip("Send Feedback", btnSendFeedback).bind();

                    btnMute = new Button();
                    btnMute.styleProvider = null;
                    btnMute.defaultIcon = SpriteFactory.getStarlingImage("ICON_BELL");
                    new HoverTooltip("Play/Pause Music", btnMute).bind();

                    lblCoords = new Label();
                    updateCoordinates();

                    minimapTools.addChild(btnMinimapZoom);
                    minimapTools.addChild(btnFind);
                    minimapTools.addChild(btnSendFeedback);
                    minimapTools.addChild(btnMute);
                    minimapTools.addChild(lblCoords);
                }

                miniMapFilters = new LayoutGroup();
                {
                    var miniMapFiltersLayoutData: AnchorLayoutData = new AnchorLayoutData(NaN, NaN, NaN, TribalHeroTheme.PADDING_DEFAULT);
                    miniMapFiltersLayoutData.leftAnchorDisplayObject = minimap;
                    miniMapFilters.layoutData = miniMapFiltersLayoutData;

                    var miniMapFiltersLayout:VerticalLayout = new VerticalLayout();
                    miniMapFiltersLayout.gap = TribalHeroTheme.PADDING_DEFAULT;
                    miniMapFilters.layout = miniMapFiltersLayout;
                    for each (var legendPanel: DisplayObject in miniMapDrawer.getLegendPanels()) {
                        miniMapFilters.addChild(legendPanel);
                    }
                }

                minimapContainer.name = "Minimap Container";
                minimapContainer.layout = new AnchorLayout();

                miniMapContainerLayoutDataUnzoomed = new AnchorLayoutData(NaN, 20, 20);
                miniMapContainerLayoutDataZoomed = new AnchorLayoutData(NaN, NaN, NaN, NaN, 0, 0);

                minimapContainer.layoutData = miniMapContainerLayoutDataUnzoomed;

                minimapContainer.addChild(minimapTools);
                minimapContainer.addChild(minimap);
            }

            addChild(map);
            addChild(minimapContainer);
            addChild(menuContainer);

            map.update();
        }

        private function onCityPickerListChange(event: Event): void {
            if (!cityPickerList.selectedItem) {
                vm.selectedCity = null;
            }
            else {
                vm.selectedCity = vm.cities.get(cityPickerList.selectedItem.cityId);
            }
        }

        private function initializeCityPickerListData(): ListCollection {
            vm.cities.addEventListener(BinaryListEvent.ADDED, onCityAdded);
            vm.cities.addEventListener(BinaryListEvent.REMOVED, onCityRemoved);

            cityPickerListData = new ListCollection();

            for each (var city: City in vm.cities.toArray().sortOn("id")) {
                addCityToPickerListData(city);
            }

            return cityPickerListData;
        }

        private function addCityToPickerListData(city: City): void {
            cityPickerListData.addItem({
                cityId: city.id,
                label: city.name
            });
        }

        private function onMiniMapNavigateToPoint(): void {
            closeWorldMap();
        }

        private function onTriggerMinimapZoom(event: Event): void {
            if (isMinimapZoomed) {
                map.camera.goToCue();
                closeWorldMap();
            }
            else {
                map.camera.cue();
                openWorldMap();
            }
        }


        private function openWorldMap(): void {
            isMinimapZoomed = true;
            // TODO: Calculate proper size
            minimap.width = 800;
            minimap.height = 500;
            minimapContainer.layoutData = miniMapContainerLayoutDataZoomed;
            minimapContainer.addChild(miniMapFilters);
            map.disableMapQueries(true);
            minimap.showPointers(true);
        }

        private function closeWorldMap(): void {
            isMinimapZoomed = false;
            minimap.width = NaN;
            minimap.height = NaN;
            minimapContainer.layoutData = miniMapContainerLayoutDataUnzoomed;
            minimapContainer.removeChild(miniMapFilters);
            map.disableMapQueries(false);
            minimap.showPointers(false);
        }

        override public function dispose(): void {
            super.dispose();

            vm.cities.removeEventListener(BinaryListEvent.ADDED, onCityAdded);
            vm.cities.removeEventListener(BinaryListEvent.REMOVED, onCityRemoved);
            map.camera.removeEventListener(Camera.ON_MOVE, updateCoordinates);
        }

        private function onCityRemoved(event: BinaryListEvent): void {
            var city: City = event.item;
            for (var i: int = 0; i < cityPickerListData.length; i++) {
                if (cityPickerListData.getItemAt(i).cityId !== city.id) {
                    continue;
                }

                cityPickerListData.removeItemAt(i);
                break;
            }
        }

        private function onCityAdded(event: BinaryListEvent): void {
            var city: City = event.item;
            addCityToPickerListData(city);
        }

        private function updateCoordinates(e: Event = null): void {
            var point: Position = map.camera.mapCenter().toPosition();

            lblCoords.text = formatString("({0},{1})", point.x, point.y);
        }

        private function getCityPickerIndex(city: City): int {
            if (city == null) {
                return -1;
            }

            for (var i:int = 0; i < cityPickerListData.length; i++) {
                if (cityPickerListData.getItemAt(i).cityId == city.id) {
                    return i;
                }
            }

            return -1;
        }
    }
}
