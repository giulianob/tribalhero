package src.Map.MiniMapDrawers
{
import flash.display.*;
import flash.geom.*;

import org.aswing.JToggleButton;

import src.Constants;
import src.Global;
import src.Map.MiniMap.LegendGroups.MiniMapGroupCity;
import src.Map.MiniMap.MiniMapLegendPanel;
import src.Map.MiniMap.MiniMapRegionObject;
import src.Util.StringHelper;

public class MiniMapCityTribe implements IMiniMapObjectDrawer
	{
        private var cityButton: JToggleButton = new JToggleButton();
        private var selfButton: JToggleButton = new JToggleButton();
        private var otherButton: JToggleButton = new JToggleButton();
        private var noneButton: JToggleButton = new JToggleButton();

        private var DEFAULT_COLORS : * = MiniMapGroupCity.DEFAULT_COLORS;

		public function applyObject(obj: MiniMapRegionObject) : void {

			if (Global.map.cities.get(obj.groupId)) {
                if(cityButton.isSelected()) return;
                obj.setIcon(new DOT_SPRITE);
				obj.transform.colorTransform = new ColorTransform();
			} else if(Constants.session.tribe.isInTribe(obj.extraProps.tribeId)) {
                if(selfButton.isSelected()) return;
                obj.setIcon(new DOT_SPRITE);
				obj.transform.colorTransform = new ColorTransform(.5, .5, .5, 1, DEFAULT_COLORS[1].r, DEFAULT_COLORS[1].g, DEFAULT_COLORS[1].b);
			} else if (obj.extraProps.tribeId>0) {
                if(otherButton.isSelected()) return;
                obj.setIcon(new DOT_SPRITE);
				obj.transform.colorTransform = new ColorTransform(.5, .5, .5, 1, DEFAULT_COLORS[4].r, DEFAULT_COLORS[4].g, DEFAULT_COLORS[4].b);
			} else {
                if(noneButton.isSelected()) return;
                obj.setIcon(new DOT_SPRITE);
				obj.transform.colorTransform = new ColorTransform(.5, .5, .5, 1, DEFAULT_COLORS[0].r, DEFAULT_COLORS[0].g, DEFAULT_COLORS[0].b);
			}
		}
		
		public function applyLegend(legend: MiniMapLegendPanel) : void {
			var icon: DisplayObject = new DOT_SPRITE;
			legend.addToggleButton(cityButton, StringHelper.localize("MINIMAP_LEGEND_CITY"), icon);

			icon = new DOT_SPRITE;
			icon.transform.colorTransform = new ColorTransform(.5, .5, .5, 1, DEFAULT_COLORS[1].r, DEFAULT_COLORS[1].g, DEFAULT_COLORS[1].b);
			legend.addToggleButton(selfButton, StringHelper.localize("MINIMAP_LEGEND_TRIBE_SELF"), icon);
			
			icon = new DOT_SPRITE;
			icon.transform.colorTransform = new ColorTransform(.5, .5, .5, 1, DEFAULT_COLORS[4].r, DEFAULT_COLORS[4].g, DEFAULT_COLORS[4].b);
			legend.addToggleButton(otherButton, StringHelper.localize("MINIMAP_LEGEND_TRIBE_OTHER_IN_TRIBE"), icon);
			
			icon = new DOT_SPRITE;
			icon.transform.colorTransform = new ColorTransform(.5, .5, .5, 1, DEFAULT_COLORS[0].r, DEFAULT_COLORS[0].g, DEFAULT_COLORS[0].b);
			legend.addToggleButton(noneButton, StringHelper.localize("MINIMAP_LEGEND_TRIBE_OTHER_NO_TRIBE"), icon);
			
		}



        public function addOnChangeListener(callback:Function):void {
            cityButton.addActionListener(callback);
            selfButton.addActionListener(callback);
            otherButton.addActionListener(callback);
            noneButton.addActionListener(callback);
        }
    }
}