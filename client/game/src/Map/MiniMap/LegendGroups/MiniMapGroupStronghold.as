/**
 * Created with IntelliJ IDEA.
 * User: OscarMike
 * Date: 2/23/14
 * Time: 1:09 AM
 * To change this template use File | Settings | File Templates.
 */
package src.Map.MiniMap.LegendGroups {
import flash.events.Event;

import src.Map.MiniMap.MiniMapLegendPanel;
import src.Map.MiniMap.MiniMapRegionObject;
import src.Map.MiniMapDrawers.*;

public class MiniMapGroupStronghold {
    private var filter : IMiniMapObjectDrawer = new MiniMapStrongholdDrawer();
    private var legendPanel : MiniMapLegendPanel = new MiniMapLegendPanel();
    private var callback : Function = null;


    public function MiniMapGroupStronghold() {
     /*   var button:JButton = new JButton(StringHelper.localize("MINIMAP_LEGEND_STRONGHOLD"));
        GameLookAndFeel.changeClass(button, "GameJBoxButton");
        button.addActionListener(onChange);
        legendPanel.addRaw(button);*/

        filter.addOnChangeListener(onChange);
        filter.applyLegend(legendPanel);
    }

    private function onChange(e:Event): void {
        if(callback!=null) callback(e);
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
