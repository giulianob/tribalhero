package src.FeathersUI.MiniMapDrawer.Drawers {
    import src.FeathersUI.MiniMapDrawer.MiniMapLegendPanel;
    import src.FeathersUI.MiniMap.MiniMapRegionObject;

    public interface IMiniMapObjectDrawer {
        function applyObject(obj: MiniMapRegionObject): void;

        function applyLegend(legend: MiniMapLegendPanel): void;

        function addOnChangeListener(callback: Function): void;
    }
}
