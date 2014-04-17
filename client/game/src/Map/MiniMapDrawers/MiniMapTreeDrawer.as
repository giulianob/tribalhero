/**
 * Created with IntelliJ IDEA.
 * User: OscarMike
 * Date: 2/20/14
 * Time: 7:41 PM
 * To change this template use File | Settings | File Templates.
 */
package src.Map.MiniMapDrawers {
import org.aswing.ASColor;
import org.aswing.AsWingConstants;
import org.aswing.AssetIcon;
import org.aswing.AssetPane;
import org.aswing.JToggleButton;
import org.aswing.geom.IntDimension;

import src.Map.MiniMap.MiniMapLegendPanel;
import src.Map.MiniMap.MiniMapRegionObject;
import src.Objects.Factories.ObjectFactory;

public class MiniMapTreeDrawer implements IMiniMapObjectDrawer {
    private var toggleButton: JToggleButton = new JToggleButton();

    public function applyObject(obj:MiniMapRegionObject):void {
        if(toggleButton.isSelected()) {
            return;
        }
        var icon: MINIMAP_FOREST_ICON = ObjectFactory.getIcon("MINIMAP_FOREST_ICON") as MINIMAP_FOREST_ICON;
        icon.alpha=0.5;
        obj.setIcon(icon);
        if(obj.extraProps.camps>0) {
            icon.lvlText.text = obj.extraProps.camps.toString();
            icon.lvlText.visible = true;
        } else {
            icon.lvlText.visible = false;
        }
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
