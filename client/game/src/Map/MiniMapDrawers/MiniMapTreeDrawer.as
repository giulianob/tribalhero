package src.Map.MiniMapDrawers {
    import flash.display.DisplayObject;
    import flash.geom.ColorTransform;

    import org.aswing.JToggleButton;

    import src.Map.MiniMap.MiniMapLegendPanel;
    import src.Map.MiniMap.MiniMapRegionObject;
    import src.Map.MiniMap.MinimapDotIcon;
    import src.Objects.Factories.ObjectFactory;
    import src.Objects.Factories.SpriteFactory;

    public class MiniMapTreeDrawer implements IMiniMapObjectDrawer {
        private var toggleButton: JToggleButton = new JToggleButton();

        private static const FOREST_COLOR: uint = 0x00CC00;

        public function applyObject(obj: MiniMapRegionObject): void {
            if (toggleButton.isSelected()) {
                return;
            }

            var dotIcon: MinimapDotIcon = new MinimapDotIcon(true, FOREST_COLOR, obj.extraProps.camps);
            obj.setIcon(dotIcon);
        }

        public function applyLegend(legend: MiniMapLegendPanel): void {
            var icon: DisplayObject = SpriteFactory.getFlashSprite("MINIMAP_LARGE_CIRCLE_SPRITE");
            icon.transform.colorTransform = new ColorTransform(0, 0, 0, 1, 0, 204, 0);
            legend.addToggleButton(toggleButton, "Forest", icon);
        }

        public function addOnChangeListener(callback: Function): void {
            toggleButton.addActionListener(callback);
        }
    }
}
