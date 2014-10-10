package src.FeathersUI.MiniMapDrawer.Drawers {
    import feathers.controls.ToggleButton;

    import src.FeathersUI.MiniMapDrawer.MiniMapLegendPanel;
    import src.FeathersUI.MiniMap.MiniMapRegionObject;
    import src.FeathersUI.MiniMap.MinimapDotIcon;

    import starling.display.Image;
    import starling.events.Event;

    public class MiniMapTreeDrawer implements IMiniMapObjectDrawer {
        private var toggleButton: ToggleButton = new ToggleButton();

        private static const FOREST_COLOR: uint = 0x00CC00;

        public function applyObject(obj: MiniMapRegionObject): void {
            if (toggleButton.isSelected) {
                return;
            }

            var dotIcon: MinimapDotIcon = new MinimapDotIcon(true, FOREST_COLOR, obj.extraProps.camps);
            obj.setIcon(dotIcon);
        }

        public function applyLegend(legend: MiniMapLegendPanel): void {
            var forestIcon: Image = new MinimapDotIcon(true, FOREST_COLOR).dot;
            legend.addFilterButton(toggleButton, "Forest", forestIcon);
        }

        public function addOnChangeListener(callback: Function): void {
            toggleButton.addEventListener(Event.CHANGE, callback);
        }
    }
}
