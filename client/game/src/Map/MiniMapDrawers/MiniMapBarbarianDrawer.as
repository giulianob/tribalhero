package src.Map.MiniMapDrawers {
    import org.aswing.JToggleButton;

    import src.Map.MiniMap.MiniMapLegendPanel;
    import src.Map.MiniMap.MiniMapRegionObject;
    import src.Map.MiniMap.MinimapDotIcon;
    import src.Objects.Factories.ObjectFactory;
    import src.Objects.Factories.SpriteFactory;

    import starling.display.Image;

    public class MiniMapBarbarianDrawer implements IMiniMapObjectDrawer {
        private var toggleButton: JToggleButton = new JToggleButton();

        public function applyObject(obj: MiniMapRegionObject): void {
            if (toggleButton.isSelected()) return;
            var icon: MinimapDotIcon = new MinimapDotIcon(4, 0x0066FF, obj.extraProps.level);
            obj.setIcon(icon);
        }

        public function applyLegend(legend: MiniMapLegendPanel): void {
            var icon: MINIMAP_BARBARIAN_TRIBE_ICON = ObjectFactory.getIcon("MINIMAP_BARBARIAN_TRIBE_ICON") as MINIMAP_BARBARIAN_TRIBE_ICON;
            icon.alpha = 0.5;
            legend.addToggleButton(toggleButton, "Barbarian", icon);
        }

        public function addOnChangeListener(callback: Function): void {
            toggleButton.addActionListener(callback);
        }
    }
}
