/**
 * Created with IntelliJ IDEA.
 * User: OscarMike
 * Date: 2/20/14
 * Time: 7:41 PM
 * To change this template use File | Settings | File Templates.
 */
package src.Map.MiniMapDrawers {
import flash.geom.ColorTransform;

import org.aswing.AssetIcon;
import org.aswing.JToggleButton;

import src.Constants;
import src.Map.MiniMap.MiniMapLegendPanel;
import src.Map.MiniMap.MiniMapRegionObject;
import src.Objects.Factories.ObjectFactory;

public class MiniMapTroopDrawer implements IMiniMapObjectDrawer {
    private var friendToggleButton: JToggleButton = new JToggleButton();
    private var foeToggleButton: JToggleButton = new JToggleButton();

    public function applyObject(obj:MiniMapRegionObject):void {
        var icon: MINIMAP_TROOP_ICON;

        var friendly:Boolean = Constants.session.tribe.isInTribe(obj.extraProps.tribeId);
        if(friendly && !friendToggleButton.isSelected()) {
            icon = ObjectFactory.getIcon("MINIMAP_TROOP_ICON") as MINIMAP_TROOP_ICON;
            obj.setIcon(icon);
            // Highlight friendly troops
            obj.transform.colorTransform = new ColorTransform(0, 0, 0, 1, 255, 255, 0);
        } else if(!friendly && !foeToggleButton.isSelected()) {
            icon = ObjectFactory.getIcon("MINIMAP_TROOP_ICON") as MINIMAP_TROOP_ICON;
            obj.setIcon(icon);
        }
    }

    public function applyLegend(legend:MiniMapLegendPanel):void {
        var icon: MINIMAP_TROOP_ICON = ObjectFactory.getIcon("MINIMAP_TROOP_ICON") as MINIMAP_TROOP_ICON;
        icon.transform.colorTransform = new ColorTransform(0, 0, 0, 1, 255, 255, 0);
        legend.addToggleButton(friendToggleButton,"Friendly Troop",icon);

        icon = ObjectFactory.getIcon("MINIMAP_TROOP_ICON") as MINIMAP_TROOP_ICON;
        legend.addToggleButton(foeToggleButton,"Enemy Troop",icon);
    }

    public function addOnChangeListener(callback:Function):void {
        friendToggleButton.addActionListener(callback);
        foeToggleButton.addActionListener(callback);
    }
}
}
