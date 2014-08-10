package src.Map.MiniMapDrawers {
    import flash.display.DisplayObject;
    import flash.geom.ColorTransform;

    import org.aswing.JToggleButton;

    import src.Map.MiniMap.MiniMapLegendPanel;
    import src.Map.MiniMap.MiniMapRegionObject;
    import src.Map.MiniMap.MinimapDotIcon;
    import src.Objects.Factories.SpriteFactory;

    public class MiniMapBarbarianDrawer implements IMiniMapObjectDrawer {
        private var toggleButton: JToggleButton = new JToggleButton();

        public function applyObject(obj: MiniMapRegionObject): void {
            if (toggleButton.isSelected()) return;
            var icon: MinimapDotIcon = new MinimapDotIcon(true, 0x0066FF, obj.extraProps.level);
            obj.setIcon(icon);
        }

        public function applyLegend(legend: MiniMapLegendPanel): void {
            var icon: DisplayObject = SpriteFactory.getFlashSprite("MINIMAP_LARGE_CIRCLE_SPRITE");
            icon.transform.colorTransform = new ColorTransform(0, 0, 0, 1, 0, 102, 255);
            legend.addToggleButton(toggleButton, "Barbarian", icon);
        }

        public function addOnChangeListener(callback: Function): void {
            toggleButton.addActionListener(callback);
        }
    }
}
