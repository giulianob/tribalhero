/**
 * Created with IntelliJ IDEA.
 * User: OscarMike
 * Date: 2/23/14
 * Time: 1:09 AM
 * To change this template use File | Settings | File Templates.
 */
package src.Map.MiniMap.LegendGroups {
import flash.events.Event;

import org.aswing.JButton;

import src.Map.MiniMap.MiniMapLegendPanel;
import src.Map.MiniMap.MiniMapRegionObject;
import src.Map.MiniMapDrawers.*;
import src.UI.LookAndFeel.GameLookAndFeel;
import src.Util.StringHelper;

public class MiniMapGroupOther {
    private var treeDrawer : MiniMapTreeDrawer = new MiniMapTreeDrawer();
    private var barbarianDrawer : MiniMapBarbarianDrawer = new MiniMapBarbarianDrawer();
    private var legendPanel : MiniMapLegendPanel = new MiniMapLegendPanel();
    private var callback : Function = null;

    public function MiniMapGroupOther() {
        treeDrawer.addOnChangeListener(onChange);
        treeDrawer.applyLegend(legendPanel);

        barbarianDrawer.addOnChangeListener(onChange);
        barbarianDrawer.applyLegend(legendPanel);
    }

    private function onChange(e:Event): void {
        if(callback!=null) callback(e);
    }

    public function applyForest(obj:MiniMapRegionObject):void {
        treeDrawer.applyObject(obj);
    }

    public function applyBarbarian(obj:MiniMapRegionObject):void {
        barbarianDrawer.applyObject(obj);
    }

    public function addOnChangeListener(callback:Function):void {
        this.callback = callback;
    }

    public function getLegendPanel(): MiniMapLegendPanel {
        return legendPanel;
    }


}
}
