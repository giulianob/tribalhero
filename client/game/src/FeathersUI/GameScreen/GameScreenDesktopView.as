package src.FeathersUI.GameScreen {
    import feathers.controls.Button;
    import feathers.controls.Label;
    import feathers.controls.LayoutGroup;
    import feathers.controls.Screen;
    import feathers.layout.AnchorLayout;
    import feathers.layout.AnchorLayoutData;
    import feathers.layout.HorizontalLayout;
    import feathers.layout.VerticalLayout;

    import src.Constants;

    import src.FeathersUI.Controls.HoverTooltip;
    import src.FeathersUI.Map.MapView;
    import src.FeathersUI.MiniMap.MiniMapView;
    import src.FeathersUI.MiniMapDrawer.MiniMapDrawer;
    import src.FeathersUI.Themes.TribalHeroTheme;
    import src.Map.Camera;
    import src.Map.Position;
    import src.Objects.Factories.SpriteFactory;

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

        public function GameScreenDesktopView(vm: GameScreenVM, mapView: MapView, miniMap: MiniMapView, miniMapDrawer: MiniMapDrawer) {
            this.vm = vm;
            this.map = mapView;
            this.minimap = miniMap;
            this.miniMapDrawer = miniMapDrawer;
        }

        override protected function initialize():void
        {
            super.initialize();

            map.camera.addEventListener(Camera.ON_MOVE, updateCoordinates);

            this.layout = new AnchorLayout();

            minimapContainer = new LayoutGroup();
            {
                minimap.layoutData = new AnchorLayoutData(NaN, NaN, 0, 0);

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

            map.update();
        }

        private function onTriggerMinimapZoom(event: Event): void {
            isMinimapZoomed = !isMinimapZoomed;

            if (isMinimapZoomed) {
                minimap.width = 800;
                minimap.height = 500;
                minimapContainer.layoutData = miniMapContainerLayoutDataZoomed;
                minimapContainer.addChild(miniMapFilters);
            }
            else {
                minimap.width = NaN;
                minimap.height = NaN;
                minimapContainer.layoutData = miniMapContainerLayoutDataUnzoomed;
                minimapContainer.removeChild(miniMapFilters);
            }
        }

        override public function dispose(): void {
            super.dispose();

            map.camera.removeEventListener(Camera.ON_MOVE, updateCoordinates);
        }

        override public function validate(): void {
            super.validate();
        }

        private function updateCoordinates(e: Event = null): void {
            var point: Position = map.camera.mapCenter().toPosition();

            lblCoords.text = formatString("({0},{1})", point.x, point.y);
        }
    }
}
