package src.FeathersUI.MiniMapDrawer.Drawers {

    import feathers.controls.ToggleButton;

    import src.FeathersUI.MiniMapDrawer.MiniMapLegendPanel;
    import src.FeathersUI.MiniMap.MiniMapRegionObject;
    import src.FeathersUI.MiniMap.MinimapDotIcon;

    import starling.display.Image;
    import starling.events.Event;

    public class MiniMapBarbarianDrawer implements IMiniMapObjectDrawer {
        private var toggleButton: ToggleButton = new ToggleButton();

        private static const BARBARIAN_ICON_COLOR: int = 0x0066FF;

        public function applyObject(obj: MiniMapRegionObject): void {
            if (toggleButton.isSelected) {
                return;
            }

            obj.setIcon(new MinimapDotIcon(true, BARBARIAN_ICON_COLOR, obj.extraProps.level));
        }

        public function applyLegend(legend: MiniMapLegendPanel): void {
            var icon: Image = new MinimapDotIcon(true, BARBARIAN_ICON_COLOR).dot;

            legend.addFilterButton(toggleButton, "Barbarian", icon);
        }

        public function addOnChangeListener(callback: Function): void {
            toggleButton.addEventListener(Event.TRIGGERED, callback);
        }
    }
}