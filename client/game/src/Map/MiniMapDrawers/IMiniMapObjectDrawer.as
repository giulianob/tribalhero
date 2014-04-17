/**
 * Created with IntelliJ IDEA.
 * User: OscarMike
 * Date: 2/18/14
 * Time: 8:41 PM
 * To change this template use File | Settings | File Templates.
 */
package src.Map.MiniMapDrawers {
import src.Map.MiniMap.MiniMapLegendPanel;
import src.Map.MiniMap.MiniMapRegionObject;

public interface IMiniMapObjectDrawer {
    function applyObject(obj: MiniMapRegionObject) : void;
    function applyLegend(legend: MiniMapLegendPanel) : void;
    function addOnChangeListener(callback : Function) : void;
}
}
