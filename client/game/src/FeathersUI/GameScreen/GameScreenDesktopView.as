package src.FeathersUI.GameScreen {
    import feathers.controls.Screen;
    import feathers.layout.AnchorLayout;
    import feathers.layout.AnchorLayoutData;

    import flash.utils.setInterval;

    import flash.utils.setTimeout;

    import src.Map.Map;
    import src.Map.MiniMap.MiniMap;

    public class GameScreenDesktopView extends Screen {
        private var vm: GameScreenVM;
        private var map: Map;
        private var minimap: MiniMap;

        public function GameScreenDesktopView(vm: GameScreenVM) {
            this.vm = vm;
            this.map = vm.map;
            this.minimap = vm.minimap;
        }

        override protected function initialize():void
        {
            super.initialize();

            this.layout = new AnchorLayout();

            var minimapAnchorLayoutData: AnchorLayoutData = new AnchorLayoutData();
            minimapAnchorLayoutData.bottom = 20;
            minimapAnchorLayoutData.right = 20;
            this.minimap.layoutData = minimapAnchorLayoutData;

            addChild(map);
            addChild(this.minimap);


            trace("Init desktop view " + width + "x" + height);

            setInterval(function() {
                trace("Map Pos:" + map.x + "," + map.y);
                trace("Map Size:" + map.width + "x" + map.height);
            }, 5000);
        }
        override public function validate(): void {
            super.validate();

            trace("Validate desktop view " + width + "x" + height);
        }
    }
}
