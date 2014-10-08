package src.FeathersUI.MiniMapDrawer.LegendGroups {
    import src.FeathersUI.MiniMapDrawer.Drawers.MiniMapBarbarianDrawer;
    import src.FeathersUI.MiniMapDrawer.Drawers.MiniMapTreeDrawer;

    import src.FeathersUI.MiniMapDrawer.MiniMapLegendPanel;
    import src.FeathersUI.MiniMap.MiniMapRegionObject;

    import starling.events.Event;

    public class MiniMapGroupOther {
        private var treeDrawer : MiniMapTreeDrawer = new MiniMapTreeDrawer();
        private var barbarianDrawer : MiniMapBarbarianDrawer = new MiniMapBarbarianDrawer();
        private var legendPanel : MiniMapLegendPanel = new MiniMapLegendPanel();
        private var callback : Function = null;

        public function MiniMapGroupOther() {
            treeDrawer.addOnChangeListener(onChange);
            treeDrawer.applyLegend(legendPanel);

            barbarianDrawer.addOnChangeListener(onChange);
            barbarianDrawer.applyLegend(legendPanel);
        }

        private function onChange(e: Event): void {
            if (callback != null) {
                callback();
            }
        }

        public function applyForest(obj:MiniMapRegionObject):void {
            treeDrawer.applyObject(obj);
        }

        public function applyBarbarian(obj:MiniMapRegionObject):void {
            barbarianDrawer.applyObject(obj);
        }

        public function addOnChangeListener(callback:Function):void {
            this.callback = callback;
        }

        public function getLegendPanel(): MiniMapLegendPanel {
            return legendPanel;
        }


    }
}
