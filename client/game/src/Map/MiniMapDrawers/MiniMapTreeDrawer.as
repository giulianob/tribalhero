package src.Map.MiniMapDrawers {
    import org.aswing.JToggleButton;

    import src.Map.MiniMap.MiniMapLegendPanel;
    import src.Map.MiniMap.MiniMapRegionObject;
    import src.Map.MiniMap.MinimapDotIcon;
    import src.Objects.Factories.ObjectFactory;

    public class MiniMapTreeDrawer implements IMiniMapObjectDrawer {
    private var toggleButton: JToggleButton = new JToggleButton();

    private static const FOREST_COLOR: uint = 0x00CC00;

    public function applyObject(obj:MiniMapRegionObject):void {
        if(toggleButton.isSelected()) {
            return;
        }

        var dotIcon: MinimapDotIcon = new MinimapDotIcon(4, FOREST_COLOR, obj.extraProps.camps);
        obj.setIcon(dotIcon);
    }

    public function applyLegend(legend:MiniMapLegendPanel):void {
        var icon: MINIMAP_FOREST_ICON = ObjectFactory.getIcon("MINIMAP_FOREST_ICON") as MINIMAP_FOREST_ICON;
        icon.alpha=0.5;
        icon.removeChild(icon.lvlText);
        legend.addToggleButton(toggleButton,"Forest",icon);
    }

    public function addOnChangeListener(callback:Function):void {
        toggleButton.addActionListener(callback);
    }
}
}
