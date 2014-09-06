package src.FeathersUI.GameScreen {
    import feathers.controls.LayoutGroup;
    import feathers.controls.Screen;
    import feathers.layout.AnchorLayout;
    import feathers.layout.AnchorLayoutData;
    import feathers.layout.HorizontalLayout;

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

            var minimapTools: LayoutGroup = new LayoutGroup();
            var minimapToolsLayout: HorizontalLayout = new HorizontalLayout();
            minimapToolsLayout.gap = 10;
            minimapTools.layout = minimapToolsLayout;



            //minimapTools.addChild()

            addChild(map);
            addChild(minimap);
            addChild(minimapTools);
        }

        override public function validate(): void {
            super.validate();
        }
    }
}
