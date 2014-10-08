package src.FeathersUI.MiniMapDrawer.Drawers {
    import feathers.controls.ToggleButton;

    import src.Constants;
    import src.FeathersUI.MiniMapDrawer.MiniMapLegendPanel;
    import src.FeathersUI.MiniMap.MiniMapRegionObject;
    import src.FeathersUI.MiniMap.MinimapDotIcon;

    import starling.display.Image;
    import starling.events.Event;

    public class MiniMapTroopDrawer implements IMiniMapObjectDrawer {
        private var friendToggleButton: ToggleButton = new ToggleButton();
        private var foeToggleButton: ToggleButton = new ToggleButton();

        private const FRIENDLY_COLOR: uint = 0xFFFF00;
        private const NON_FRIENDLY_COLOR: uint = 0x990000;

        public function applyObject(obj: MiniMapRegionObject): void {
            var icon: MinimapDotIcon;

            var friendly: Boolean = Constants.session.tribe.isInTribe(obj.extraProps.tribeId);
            if (friendly && !friendToggleButton.isSelected) {
                icon = new MinimapDotIcon(false, FRIENDLY_COLOR);
            } else if (!friendly && !foeToggleButton.isSelected) {
                icon = new MinimapDotIcon(false, NON_FRIENDLY_COLOR);
            }

            obj.setIcon(icon);
        }

        public function applyLegend(legend: MiniMapLegendPanel): void {
            var friendlyIcon: Image = new MinimapDotIcon(true, FRIENDLY_COLOR).dot;
            legend.addFilterButton(friendToggleButton, "Friendly Troop", friendlyIcon);

            var nonFriendlyIcon: Image = new MinimapDotIcon(true, NON_FRIENDLY_COLOR).dot;
            legend.addFilterButton(foeToggleButton, "Enemy Troop", nonFriendlyIcon);
        }

        public function addOnChangeListener(callback: Function): void {
            friendToggleButton.addEventListener(Event.TRIGGERED, callback);
            foeToggleButton.addEventListener(Event.TRIGGERED, callback);
        }
    }
}
