package src.FeathersUI.GameScreen {
    import feathers.controls.Button;
    import feathers.controls.Label;
    import feathers.controls.LayoutGroup;
    import feathers.controls.Screen;
    import feathers.layout.AnchorLayout;
    import feathers.layout.AnchorLayoutData;
    import feathers.layout.HorizontalLayout;
    import feathers.layout.VerticalLayout;

    import flash.events.Event;

    import src.FeathersUI.Controls.HoverTooltip;
    import src.FeathersUI.Map.MapView;
    import src.Map.Camera;
    import src.Map.MiniMap.MiniMap;
    import src.Map.Position;
    import src.Objects.Factories.SpriteFactory;

    import starling.utils.formatString;

    public class GameScreenDesktopView extends Screen {
        private var vm: GameScreenVM;
        private var map: MapView;
        private var minimap: MiniMap;
        private var btnMinimapZoom: Button;
        private var btnFind: Button;
        private var btnSendFeedback: Button;
        private var btnMute: Button;
        private var lblCoords: Label;

        public function GameScreenDesktopView(vm: GameScreenVM, mapView: MapView, miniMap: MiniMap) {
            this.vm = vm;
            this.map = mapView;
            this.minimap = miniMap;
        }

        override protected function initialize():void
        {
            super.initialize();

            map.camera.addEventListener(Camera.ON_MOVE, updateCoordinates);

            this.layout = new AnchorLayout();

            var minimapContainer: LayoutGroup = new LayoutGroup();
            {
                var minimapTools: LayoutGroup = new LayoutGroup();
                {
                    var minimapToolsLayout: HorizontalLayout = new HorizontalLayout();
                    minimapToolsLayout.verticalAlign = HorizontalLayout.VERTICAL_ALIGN_MIDDLE;
                    minimapToolsLayout.gap = 10;
                    minimapTools.layout = minimapToolsLayout;

                    btnMinimapZoom = new Button();
                    btnMinimapZoom.styleProvider = null;
                    btnMinimapZoom.defaultIcon = SpriteFactory.getStarlingImage("ICON_MAP");
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

                minimapContainer.name = "Minimap Container";
                var minimapContainerLayout: VerticalLayout = new VerticalLayout();
                minimapContainerLayout.gap = 5;
                minimapContainer.layout = minimapContainerLayout;

                minimapContainer.layoutData = new AnchorLayoutData(Number.NaN, 20, 20);

                minimapContainer.addChild(minimapTools);
                minimapContainer.addChild(minimap);
            }

            addChild(map);
            addChild(minimapContainer);

            map.move();
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
