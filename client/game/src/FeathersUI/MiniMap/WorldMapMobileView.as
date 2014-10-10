package src.FeathersUI.MiniMap {
    import feathers.controls.Button;
    import feathers.controls.Drawers;
    import feathers.controls.Label;
    import feathers.controls.PanelScreen;
    import feathers.controls.ScrollContainer;
    import feathers.events.FeathersEventType;
    import feathers.layout.AnchorLayout;
    import feathers.layout.AnchorLayoutData;
    import feathers.layout.VerticalLayout;

    import src.FeathersUI.Controls.ScreenTransitionDetector;
    import src.FeathersUI.MiniMapDrawer.MiniMapDrawer;
    import src.FeathersUI.Themes.TribalHeroTheme;

    import starling.display.DisplayObject;
    import starling.events.Event;

    public class WorldMapMobileView extends PanelScreen {
        public static const EVENT_CLOSE_MINIMAP: String = "EVENT_CLOSE_MINIMAP";

        private var vm: WorldMapMobileVM;
        private var miniMapView: MiniMapView;

        private var drawer: Drawers;

        private var backButton: Button;
        private var filterButton: Button;

        private var transitionDetector: ScreenTransitionDetector;
        private var filters: ScrollContainer;
        private var mapFilter: MiniMapDrawer;

        public function WorldMapMobileView(vm: WorldMapMobileVM, miniMapView: MiniMapView, mapFilter: MiniMapDrawer) {
            super();

            this.vm = vm;
            this.miniMapView = miniMapView;
            this.mapFilter = mapFilter;
            this.transitionDetector = new ScreenTransitionDetector(this);
            this.transitionDetector.addEventListener(FeathersEventType.TRANSITION_START, onTransitionStart);
            this.transitionDetector.addEventListener(FeathersEventType.TRANSITION_COMPLETE, onTransitionComplete);

            // We dont let minimap fetch regions until transition is done
            miniMapView.enableRegionFetching(false);
            miniMapView.showScreenRect(false);
            miniMapView.backgroundRadius = 0;
        }

        private function onTransitionComplete(event: Event): void {
            miniMapView.enableRegionFetching(true);
        }

        private function onTransitionStart(event: Event): void {
            miniMapView.enableRegionFetching(false);
        }

        override public function dispose(): void {
            super.dispose();

            this.transitionDetector.dispose();
        }

        override protected function initialize(): void {
            super.initialize();

            this.layout = new AnchorLayout();

            this.backButtonHandler = onBackButtonHandler;

            this.backButton = new Button();
            this.backButton.label = t("STR_BACK_BUTTON");
            this.backButton.styleNameList.add(Button.ALTERNATE_NAME_BACK_BUTTON);
            this.backButton.addEventListener(Event.TRIGGERED, onBackButtonTriggered);

            this.filterButton = new Button();
            this.filterButton.label = t("STR_FILTER_BUTTON");
            this.filterButton.addEventListener(Event.TRIGGERED, onFilterButtonTriggered);

            this.headerProperties.leftItems = new <DisplayObject>[ this.backButton ];
            this.headerProperties.rightItems = new <DisplayObject>[ this.filterButton ];

            drawer = new Drawers(miniMapView);
            drawer.layoutData = new AnchorLayoutData(0, 0, 0, 0);

            this.filters = new ScrollContainer();
            var verticalLayout: VerticalLayout = new VerticalLayout();
            verticalLayout.horizontalAlign = VerticalLayout.HORIZONTAL_ALIGN_JUSTIFY;
            verticalLayout.gap = TribalHeroTheme.PADDING_DEFAULT;
            verticalLayout.padding = TribalHeroTheme.PADDING_DEFAULT;

            this.filters.layout = verticalLayout;

            for each (var legendPanel: DisplayObject in mapFilter.getLegendPanels()) {
                this.filters.addChild(legendPanel);
            }

            drawer.openGesture = Drawers.OPEN_GESTURE_NONE;
            drawer.rightDrawer = this.filters;

            addChild(drawer);
        }

        private function onFilterButtonTriggered(event: Event): void {
            this.drawer.toggleRightDrawer();
        }

        private function onBackButtonTriggered(event: Event): void {
            this.onBackButtonHandler();
        }

        private function onBackButtonHandler(): void {
            this.dispatchEventWith(EVENT_CLOSE_MINIMAP);
        }
    }
}
