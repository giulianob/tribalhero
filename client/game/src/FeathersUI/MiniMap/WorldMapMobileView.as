package src.FeathersUI.MiniMap {
    import feathers.controls.Button;
    import feathers.controls.Drawers;
    import feathers.controls.PanelScreen;
    import feathers.layout.AnchorLayout;
    import feathers.layout.AnchorLayoutData;

    import starling.display.DisplayObject;

    import starling.events.Event;

    public class WorldMapMobileView extends PanelScreen {
        public static const EVENT_CLOSE_MINIMAP: String = "EVENT_CLOSE_MINIMAP";

        private var vm: WorldMapMobileVM;
        private var miniMapView: MiniMapView;

        private var backButton: Button;

        public function WorldMapMobileView(vm: WorldMapMobileVM, miniMapView: MiniMapView) {
            super();

            this.vm = vm;
            this.miniMapView = miniMapView;
        }

        override protected function initialize(): void {
            super.initialize();

            this.layout = new AnchorLayout();

            this.backButtonHandler = onBackButtonHandler;

            this.backButton = new Button();
            this.backButton.label = t("STR_BACK_BUTTON");
            this.backButton.styleNameList.add(Button.ALTERNATE_NAME_BACK_BUTTON);
            this.backButton.addEventListener(Event.TRIGGERED, onBackButtonTriggered);

            this.headerProperties.leftItems = new <DisplayObject>[ this.backButton ];

            var drawer: Drawers = new Drawers(miniMapView);
            drawer.layoutData = new AnchorLayoutData(0, 0, 0, 0);

            addChild(drawer);
        }

        private function onBackButtonTriggered(event: Event): void {
            this.onBackButtonHandler();
        }

        private function onBackButtonHandler(): void {
            this.dispatchEventWith(EVENT_CLOSE_MINIMAP);
        }
    }
}
