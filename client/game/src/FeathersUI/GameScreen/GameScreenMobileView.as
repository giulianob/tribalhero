package src.FeathersUI.GameScreen {
    import feathers.controls.Button;
    import feathers.controls.Drawers;
    import feathers.controls.LayoutGroup;
    import feathers.controls.Screen;
    import feathers.layout.AnchorLayout;
    import feathers.layout.AnchorLayoutData;
    import feathers.layout.TiledRowsLayout;

    import src.FeathersUI.Controls.ClippedLayoutGroup;
    import src.FeathersUI.Map.MapView;
    import src.FeathersUI.MiniMap.MiniMapView;
    import src.FeathersUI.Themes.TribalHeroTheme;
    import src.Map.City;
    import src.Map.MiniMap.MiniMap;
    import src.Map.ScreenPosition;
    import src.Objects.Factories.SpriteFactory;

    import starling.events.Event;

    public class GameScreenMobileView extends Screen {
        public static const EVENT_VIEW_MINIMAP: String = "EVENT_VIEW_MINIMAP";

        private var vm: GameScreenVM;
        private var map: MapView;
        private var drawers: Drawers;

        public function GameScreenMobileView(vm: GameScreenVM, mapView: MapView) {
            this.vm = vm;
            this.map = mapView;
        }

        override protected function initialize():void
        {
            super.initialize();

            var menu: LayoutGroup = new LayoutGroup();
            {
                menu.name = "Game Screen Menu";
                menu.layout = new TiledRowsLayout();

                menu.addChild(createDrawerChildMenuButton("Battle Reports", "ICON_PAPER_SCROLL"));
                menu.addChild(createDrawerChildMenuButton("Ranking", "ICON_PAPER_SCROLL"));
                menu.addChild(createDrawerChildMenuButton("Store", "ICON_PAPER_SCROLL"));
                menu.addChild(createDrawerChildMenuButton("Tribe Info", "ICON_PAPER_SCROLL"));
                menu.addChild(createDrawerChildMenuButton("Tribe Forums", "ICON_PAPER_SCROLL"));
                menu.addChild(createDrawerChildMenuButton("Strongholds", "ICON_PAPER_SCROLL"));
                menu.addChild(createDrawerChildMenuButton("Profile", "ICON_PAPER_SCROLL"));
                menu.addChild(createDrawerChildMenuButton("Settings", "ICON_PAPER_SCROLL"));
                menu.addChild(createDrawerChildMenuButton("Help", "ICON_PAPER_SCROLL"));

                menu.width = 300;
            }

            var mapContainer: ClippedLayoutGroup = new ClippedLayoutGroup();
            {
                mapContainer.name = "Game Screen Drawer Container";
                mapContainer.layout = new AnchorLayout();

                var menuButton:Button = new Button();
                menuButton.styleNameList.add(Button.ALTERNATE_NAME_QUIET_BUTTON);
                menuButton.defaultIcon = SpriteFactory.getStarlingImage("ICON_PAPER_SCROLL");
                menuButton.addEventListener(Event.TRIGGERED, onMenuButtonTriggered);
                menuButton.layoutData = new AnchorLayoutData(TribalHeroTheme.PADDING_DEFAULT, NaN, NaN, TribalHeroTheme.PADDING_DEFAULT);

                var viewMiniMapButton:Button = new Button();
                viewMiniMapButton.styleNameList.add(Button.ALTERNATE_NAME_QUIET_BUTTON);
                viewMiniMapButton.defaultIcon = SpriteFactory.getStarlingImage("ICON_PAPER_SCROLL");
                viewMiniMapButton.addEventListener(Event.TRIGGERED, onViewMiniMapButtonTrigerred);
                viewMiniMapButton.layoutData = new AnchorLayoutData(NaN, TribalHeroTheme.PADDING_DEFAULT, TribalHeroTheme.PADDING_DEFAULT);

                mapContainer.addChild(map);
                mapContainer.addChild(menuButton);
                mapContainer.addChild(viewMiniMapButton);
            }

            drawers = new Drawers(mapContainer);
            drawers.leftDrawer = menu;
            drawers.openGesture = Drawers.OPEN_GESTURE_NONE;

            addChild(drawers);

            map.update();
        }

        private function createDrawerChildMenuButton(text: String, icon: String): Button {
            var button: Button = new Button();
            button.defaultIcon = SpriteFactory.getStarlingImage(icon);
            button.iconPosition = Button.ICON_POSITION_TOP;
            button.label = text;
            button.styleNameList.add(Button.ALTERNATE_NAME_QUIET_BUTTON);

            return button;
        }

        private function onMenuButtonTriggered(event:Event):void
        {
            drawers.toggleLeftDrawer();
        }

        private function onViewMiniMapButtonTrigerred(event: Event): void {
            this.dispatchEventWith(EVENT_VIEW_MINIMAP);
        }
    }
}
