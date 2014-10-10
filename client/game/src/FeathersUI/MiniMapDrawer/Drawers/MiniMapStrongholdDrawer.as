package src.FeathersUI.MiniMapDrawer.Drawers {
    import feathers.controls.ToggleButton;

    import src.FeathersUI.MiniMapDrawer.MiniMapLegendPanel;
    import src.FeathersUI.MiniMap.MiniMapRegionObject;
    import src.FeathersUI.MiniMap.MinimapDotIcon;

    import starling.display.Image;
    import starling.events.Event;

    public class MiniMapStrongholdDrawer implements IMiniMapObjectDrawer {
        private var toggleButton: ToggleButton = new ToggleButton();

        private static const STRONGHOLD_COLOR: uint = 0xFFCC33;

        public function applyObject(obj: MiniMapRegionObject): void {
            if (toggleButton.isSelected) {
                return;
            }

            var dotIcon: MinimapDotIcon = new MinimapDotIcon(true, STRONGHOLD_COLOR, obj.extraProps.level);
            obj.setIcon(dotIcon);
        }

        public function applyLegend(legend: MiniMapLegendPanel): void {
            var icon: Image = new MinimapDotIcon(true, STRONGHOLD_COLOR).dot;

            legend.addFilterButton(toggleButton, "Stronghold", icon);
        }

        public function addOnChangeListener(callback: Function): void {
            toggleButton.addEventListener(Event.CHANGE, callback);
        }
    }
}
