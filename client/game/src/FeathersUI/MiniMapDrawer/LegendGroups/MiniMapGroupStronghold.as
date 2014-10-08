package src.FeathersUI.MiniMapDrawer.LegendGroups {

    import src.FeathersUI.MiniMapDrawer.Drawers.IMiniMapObjectDrawer;
    import src.FeathersUI.MiniMapDrawer.Drawers.MiniMapStrongholdDrawer;
    import src.FeathersUI.MiniMapDrawer.MiniMapLegendPanel;
import src.FeathersUI.MiniMap.MiniMapRegionObject;

    import starling.events.Event;

    public class MiniMapGroupStronghold {
    private var filter : IMiniMapObjectDrawer = new MiniMapStrongholdDrawer();
    private var legendPanel : MiniMapLegendPanel = new MiniMapLegendPanel();
    private var callback : Function = null;


    public function MiniMapGroupStronghold() {
        filter.addOnChangeListener(onChange);
        filter.applyLegend(legendPanel);
    }

    private function onChange(e:Event): void {
        if (callback != null) {
            callback();
        }
    }

    public function applyStronghold(obj:MiniMapRegionObject):void {
        filter.applyObject(obj);
    }


    public function addOnChangeListener(callback:Function):void {
        this.callback = callback;
    }

    public function getLegendPanel(): MiniMapLegendPanel {
        return legendPanel;
    }


}
}
