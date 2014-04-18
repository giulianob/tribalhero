package src.Map.MiniMapDrawers
{
import flash.display.*;
import flash.geom.*;

import org.aswing.JToggleButton;

import src.Global;
import src.Map.MiniMap.LegendGroups.MiniMapGroupCity;
import src.Map.MiniMap.MiniMapLegendPanel;
import src.Map.MiniMap.MiniMapRegionObject;
import src.Util.StringHelper;

/**
	 * ...
	 * @author Anthony Lam
	 */
	public class MiniMapCityNewbie implements IMiniMapObjectDrawer
	{
        private var cityButton: JToggleButton = new JToggleButton();
        private var newbieButton: JToggleButton = new JToggleButton();
        private var saltyButton: JToggleButton = new JToggleButton();

        private var DEFAULT_COLORS : * = MiniMapGroupCity.DEFAULT_COLORS;

		public function applyObject(obj: MiniMapRegionObject) : void {
			// If it's our city, we just show a special flag

			if (Global.map.cities.get(obj.groupId)) {
                if(cityButton.isSelected()) return;
                obj.setIcon(new DOT_SPRITE);
				obj.transform.colorTransform = new ColorTransform();
			} else if(obj.extraProps.isNewbie) {
                if(newbieButton.isSelected()) return;
                obj.setIcon(new DOT_SPRITE);
				obj.transform.colorTransform = new ColorTransform(.5, .5, .5, 1, DEFAULT_COLORS[3].r, DEFAULT_COLORS[3].g, DEFAULT_COLORS[3].b);
			} else {
                if(saltyButton.isSelected()) return;
                obj.setIcon(new DOT_SPRITE);
				obj.transform.colorTransform = new ColorTransform(.5, .5, .5, 1, DEFAULT_COLORS[0].r, DEFAULT_COLORS[0].g, DEFAULT_COLORS[0].b);
			}
		}
		
		public function applyLegend(legend: MiniMapLegendPanel) : void {
			var icon: DisplayObject = new DOT_SPRITE;
			legend.addToggleButton(cityButton,StringHelper.localize("MINIMAP_LEGEND_CITY"),icon);
			
			icon = new DOT_SPRITE;
			icon.transform.colorTransform = new ColorTransform(.5, .5, .5, 1, DEFAULT_COLORS[3].r, DEFAULT_COLORS[3].g, DEFAULT_COLORS[3].b);
			legend.addToggleButton(newbieButton,StringHelper.localize("MINIMAP_LEGEND_NEWBIE_YES"),icon);
			
			icon = new DOT_SPRITE;
			icon.transform.colorTransform = new ColorTransform(.5, .5, .5, 1, DEFAULT_COLORS[0].r, DEFAULT_COLORS[0].g, DEFAULT_COLORS[0].b);
			legend.addToggleButton(saltyButton,StringHelper.localize("MINIMAP_LEGEND_NEWBIE_NO"),icon);
			
		}


        public function addOnChangeListener(callback:Function):void {
            cityButton.addActionListener(callback);
            newbieButton.addActionListener(callback);
            saltyButton.addActionListener(callback);
        }
    }
}