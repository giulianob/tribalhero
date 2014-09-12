package src.FeathersUI.GameScreen {
    import feathers.controls.Button;
    import feathers.controls.Drawers;
    import feathers.controls.LayoutGroup;
    import feathers.controls.Screen;
    import feathers.layout.AnchorLayoutData;
    import feathers.layout.TiledRowsLayout;

    import src.FeathersUI.Controls.ClippedLayoutGroup;
    import src.FeathersUI.Map.MapView;
    import src.Map.MiniMap.MiniMap;
    import src.Objects.Factories.SpriteFactory;

    import starling.events.Event;

    public class GameScreenMobileView extends Screen {
        private var vm: GameScreenVM;
        private var map: MapView;
        private var miniMap: MiniMap;
        private var drawers: Drawers;

        public function GameScreenMobileView(vm: GameScreenVM, mapView: MapView, miniMap: MiniMap) {
            this.vm = vm;
            this.map = mapView;
            this.miniMap = miniMap;
        }

        override protected function initialize():void
        {
            super.initialize();

            var menu: LayoutGroup = new LayoutGroup();
            {
                menu.name = "Game Screen Menu";
                menu.layout = new TiledRowsLayout();

                menu.addChild(createMenuButton("Battle Reports", "ICON_PAPER_SCROLL"));
                menu.addChild(createMenuButton("Ranking", "ICON_PAPER_SCROLL"));
                menu.addChild(createMenuButton("Store", "ICON_PAPER_SCROLL"));
                menu.addChild(createMenuButton("Tribe Info", "ICON_PAPER_SCROLL"));
                menu.addChild(createMenuButton("Tribe Forums", "ICON_PAPER_SCROLL"));
                menu.addChild(createMenuButton("Strongholds", "ICON_PAPER_SCROLL"));
                menu.addChild(createMenuButton("Profile", "ICON_PAPER_SCROLL"));
                menu.addChild(createMenuButton("Settings", "ICON_PAPER_SCROLL"));
                menu.addChild(createMenuButton("Help", "ICON_PAPER_SCROLL"));

                menu.width = 300;
            }

            var mapContainer: ClippedLayoutGroup = new ClippedLayoutGroup();
            {
                mapContainer.name = "Game Screen Drawer Container";
                var menuButton:Button = new Button();
                {
                    menuButton.styleNameList.add(Button.ALTERNATE_NAME_QUIET_BUTTON);
                    menuButton.defaultIcon = SpriteFactory.getStarlingImage("ICON_PAPER_SCROLL");
                    menuButton.addEventListener(Event.TRIGGERED, menuButton_triggeredHandler);

                    menuButton.layoutData = new AnchorLayoutData(10, Number.NaN, Number.NaN, 10);
                }

                mapContainer.addChild(map);
                mapContainer.addChild(menuButton);
            }

            drawers = new Drawers(mapContainer);
            drawers.leftDrawer = menu;
            drawers.openGesture = Drawers.OPEN_GESTURE_NONE;

            addChild(drawers);

            map.move();
        }

        private function createMenuButton(text: String, icon: String): Button {
            var button: Button = new Button();
            button.defaultIcon = SpriteFactory.getStarlingImage(icon);
            button.iconPosition = Button.ICON_POSITION_TOP;
            button.label = text;
            button.styleNameList.add(Button.ALTERNATE_NAME_QUIET_BUTTON);

            return button;
        }

        private function menuButton_triggeredHandler(event:Event):void
        {
            drawers.toggleLeftDrawer();
        }
    }
}
