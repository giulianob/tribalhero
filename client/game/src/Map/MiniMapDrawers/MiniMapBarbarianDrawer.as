/**
 * Created with IntelliJ IDEA.
 * User: OscarMike
 * Date: 2/20/14
 * Time: 7:41 PM
 * To change this template use File | Settings | File Templates.
 */
package src.Map.MiniMapDrawers {
import org.aswing.AssetIcon;
import org.aswing.JToggleButton;

import src.Map.MiniMap.MiniMapLegendPanel;
import src.Map.MiniMap.MiniMapRegionObject;
import src.Objects.Factories.ObjectFactory;

public class MiniMapBarbarianDrawer implements IMiniMapObjectDrawer {
    private var toggleButton: JToggleButton = new JToggleButton();

    public function applyObject(obj:MiniMapRegionObject):void {
        if(toggleButton.isSelected()) return;
        var icon: MINIMAP_BARBARIAN_TRIBE_ICON = ObjectFactory.getIcon("MINIMAP_BARBARIAN_TRIBE_ICON") as MINIMAP_BARBARIAN_TRIBE_ICON;
        icon.alpha=0.5;
        obj.setIcon(icon);
        icon.lvlText.text = obj.extraProps.level.toString();
    }

    public function applyLegend(legend:MiniMapLegendPanel):void {
        var icon: MINIMAP_BARBARIAN_TRIBE_ICON = ObjectFactory.getIcon("MINIMAP_BARBARIAN_TRIBE_ICON") as MINIMAP_BARBARIAN_TRIBE_ICON;
        icon.alpha=0.5;
        icon.removeChild(icon.lvlText);
        legend.addToggleButton(toggleButton,"Barbarian",icon);
    }

    public function addOnChangeListener(callback:Function):void {
        toggleButton.addActionListener(callback);
    }
}
}
