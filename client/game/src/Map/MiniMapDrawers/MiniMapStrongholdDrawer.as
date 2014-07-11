package src.Map.MiniMapDrawers {
    import flash.display.DisplayObject;
    import flash.geom.ColorTransform;

    import org.aswing.JToggleButton;

    import src.Map.MiniMap.MiniMapLegendPanel;
    import src.Map.MiniMap.MiniMapRegionObject;
    import src.Map.MiniMap.MinimapDotIcon;
    import src.Objects.Factories.ObjectFactory;
    import src.Objects.Factories.SpriteFactory;

    public class MiniMapStrongholdDrawer implements IMiniMapObjectDrawer {
        private var toggleButton: JToggleButton = new JToggleButton();

        private static const STRONGHOLD_COLOR: uint = 0xFFCC33;

        public function applyObject(obj: MiniMapRegionObject): void {
            if (toggleButton.isSelected()) {
                return;
            }

            var dotIcon: MinimapDotIcon = new MinimapDotIcon(true, STRONGHOLD_COLOR, obj.extraProps.level);
            obj.setIcon(dotIcon);
        }

        public function applyLegend(legend: MiniMapLegendPanel): void {
            var icon: DisplayObject = SpriteFactory.getFlashSprite("MINIMAP_LARGE_CIRCLE_SPRITE");
            icon.transform.colorTransform = new ColorTransform(0, 0, 0, 1, 255, 204, 51);
            legend.addToggleButton(toggleButton, "Stronghold", icon);
        }

        public function addOnChangeListener(callback: Function): void {
            toggleButton.addActionListener(callback);
        }
    }
}
