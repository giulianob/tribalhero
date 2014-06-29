package src.Map.MiniMapDrawers {
    import org.aswing.JToggleButton;

    import src.Map.MiniMap.MiniMapLegendPanel;
    import src.Map.MiniMap.MiniMapRegionObject;
    import src.Map.MiniMap.MinimapDotIcon;
    import src.Objects.Factories.ObjectFactory;

    public class MiniMapStrongholdDrawer implements IMiniMapObjectDrawer{
    private var toggleButton : JToggleButton = new JToggleButton();

    private static const STRONGHOLD_COLOR: uint = 0xFFCC33;

    public function applyObject(obj: MiniMapRegionObject) : void {
        if(toggleButton.isSelected()) {
            return;
        }

        var dotIcon: MinimapDotIcon = new MinimapDotIcon(4, STRONGHOLD_COLOR, obj.extraProps.level);
        obj.setIcon(dotIcon);
    }

    public function applyLegend(legend:MiniMapLegendPanel):void {
        var icon: MINIMAP_STRONGHOLD_ICON = ObjectFactory.getIcon("MINIMAP_STRONGHOLD_ICON") as MINIMAP_STRONGHOLD_ICON;
        icon.alpha=0.5;
        icon.removeChild(icon.lvlText);
        legend.addToggleButton(toggleButton,"Stronghold",icon);
    }

    public function addOnChangeListener(callback:Function):void {
        toggleButton.addActionListener(callback);
    }
}
}
