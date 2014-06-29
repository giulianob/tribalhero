package src.Map.MiniMapDrawers
{
import flash.display.*;
import flash.geom.*;

import org.aswing.AssetIcon;

import org.aswing.JToggleButton;

import src.Global;
import src.Map.MiniMap.LegendGroups.MiniMapGroupCity;
import src.Map.MiniMap.MiniMapLegendPanel;
import src.Map.MiniMap.MiniMapRegionObject;
    import src.Objects.Factories.SpriteFactory;
    import src.Util.StringHelper;

    import starling.display.Image;

    public class MiniMapCityAlignment implements IMiniMapObjectDrawer
	{
        private var cityButton: JToggleButton = new JToggleButton();
        private var ap20Button: JToggleButton = new JToggleButton();
        private var ap40Button: JToggleButton = new JToggleButton();
        private var ap60Button: JToggleButton = new JToggleButton();
        private var ap80Button: JToggleButton = new JToggleButton();
        private var ap100Button: JToggleButton = new JToggleButton();

        private var DEFAULT_COLORS : * = MiniMapGroupCity.DEFAULT_COLORS;

		public function applyObject(obj: MiniMapRegionObject) : void {

			var alignment: Number = obj.extraProps.alignment;
			var alignmentIdx: int;
            var dotSprite: Image;
			if (Global.map.cities.get(obj.groupId)) {
                // If it's our city, we just show a special flag
                if(cityButton.isSelected()) {
                    return;
                }

                dotSprite = SpriteFactory.getStarlingImage("DOT_SPRITE");
                dotSprite.color = MiniMapGroupCity.CITY_DEFAULT_COLOR;
                obj.setIcon(dotSprite);
			}
			else {
				if (alignment <= 20 && !ap20Button.isSelected()) alignmentIdx = 0;
				else if (alignment > 20 && alignment <= 40 && !ap40Button.isSelected()) alignmentIdx = 1;
				else if (alignment > 40 && alignment <= 60 && !ap60Button.isSelected()) alignmentIdx = 2;
				else if (alignment > 60 && alignment <= 80 && !ap80Button.isSelected()) alignmentIdx = 3;
				else if(alignment > 80 && !ap100Button.isSelected()) alignmentIdx = 4;
                else return;

                dotSprite = SpriteFactory.getStarlingImage("DOT_SPRITE");
                dotSprite.color = DEFAULT_COLORS[alignmentIdx].hex;
                obj.setIcon(dotSprite);
			}
		}
		
		public function applyLegend(legend: MiniMapLegendPanel) : void {
			var icon: DisplayObject = new DOT_SPRITE;
            legend.addToggleButton(cityButton,StringHelper.localize("MINIMAP_LEGEND_CITY"),icon);

			icon = new DOT_SPRITE;
            icon.transform.colorTransform = new ColorTransform(.5, .5, .5, 1, DEFAULT_COLORS[4].r, DEFAULT_COLORS[4].g, DEFAULT_COLORS[4].b);
			legend.addToggleButton(ap100Button,StringHelper.localize("MINIMAP_LEGEND_ALIGNMENT_90"),icon);
			
			icon = new DOT_SPRITE;
			icon.transform.colorTransform = new ColorTransform(.5, .5, .5, 1, DEFAULT_COLORS[3].r, DEFAULT_COLORS[3].g, DEFAULT_COLORS[3].b);
			legend.addToggleButton(ap80Button,StringHelper.localize("MINIMAP_LEGEND_ALIGNMENT_75"),icon);

			icon = new DOT_SPRITE;
			icon.transform.colorTransform = new ColorTransform(.5, .5, .5, 1, DEFAULT_COLORS[2].r, DEFAULT_COLORS[2].g, DEFAULT_COLORS[2].b);
            legend.addToggleButton(ap60Button,StringHelper.localize("MINIMAP_LEGEND_ALIGNMENT_50"),icon);

			icon = new DOT_SPRITE;
			icon.transform.colorTransform = new ColorTransform(.5, .5, .5, 1, DEFAULT_COLORS[1].r, DEFAULT_COLORS[1].g, DEFAULT_COLORS[1].b);
            legend.addToggleButton(ap40Button,StringHelper.localize("MINIMAP_LEGEND_ALIGNMENT_25"),icon);

			icon = new DOT_SPRITE;
			icon.transform.colorTransform = new ColorTransform(.5, .5, .5, 1, DEFAULT_COLORS[0].r, DEFAULT_COLORS[0].g, DEFAULT_COLORS[0].b);
            legend.addToggleButton(ap20Button,StringHelper.localize("MINIMAP_LEGEND_ALIGNMENT_10"),icon);

		}

        public function addOnChangeListener(callback:Function):void {
            cityButton.addActionListener(callback);
            ap20Button.addActionListener(callback);
            ap40Button.addActionListener(callback);
            ap60Button.addActionListener(callback);
            ap80Button.addActionListener(callback);
            ap100Button.addActionListener(callback);
        }
    }
}