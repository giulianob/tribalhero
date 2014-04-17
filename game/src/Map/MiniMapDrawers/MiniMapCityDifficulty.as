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
import src.Util.StringHelper;

public class MiniMapCityDifficulty implements IMiniMapObjectDrawer
	{
        private var cityButton: JToggleButton = new JToggleButton();
        private var strongestButton: JToggleButton = new JToggleButton();
        private var strongButton: JToggleButton = new JToggleButton();
        private var normalButton: JToggleButton = new JToggleButton();
        private var weakButton: JToggleButton = new JToggleButton();
        private var weakestButton: JToggleButton = new JToggleButton();

        private var DEFAULT_COLORS : * = MiniMapGroupCity.DEFAULT_COLORS;

        public function applyObject(obj: MiniMapRegionObject) : void {
            var icon: DisplayObject;

            if (Global.map.cities.get(obj.groupId)) {
                if(cityButton.isSelected()) return;
                icon = new DOT_SPRITE;
                obj.setIcon(icon);
                obj.transform.colorTransform = new ColorTransform();
            } else {
                icon = new DOT_SPRITE;
                // Apply the difficulty transformation to the tile
                var percDiff: Number = Number(obj.extraProps.value) / Math.max(1.0, Number(Global.gameContainer.selectedCity.value));
                var difficultyIdx: int;
                if (percDiff <= 0.2 && !weakestButton.isSelected()) difficultyIdx = 0;
                else if (percDiff > .2 && percDiff <= 0.75 && !weakButton.isSelected()) difficultyIdx = 1;
                else if (percDiff > .75 && percDiff <= 1.5 && !normalButton.isSelected()) difficultyIdx = 2;
                else if (percDiff > 1.5 && percDiff <= 1.9 && !strongButton.isSelected()) difficultyIdx = 3;
                else if (percDiff > 1.9 && !strongestButton.isSelected()) difficultyIdx = 4;
                else return;

                obj.setIcon(icon);
                obj.transform.colorTransform = new ColorTransform(.5, .5, .5, 1, DEFAULT_COLORS[difficultyIdx].r, DEFAULT_COLORS[difficultyIdx].g, DEFAULT_COLORS[difficultyIdx].b);
            }
        }

        public function applyLegend(legend: MiniMapLegendPanel) : void {
            var icon: DisplayObject = new DOT_SPRITE;
            legend.addToggleButton(cityButton,StringHelper.localize("MINIMAP_LEGEND_CITY"),icon);

            icon = new DOT_SPRITE;
            icon.transform.colorTransform = new ColorTransform(.5, .5, .5, 1, DEFAULT_COLORS[4].r, DEFAULT_COLORS[4].g, DEFAULT_COLORS[4].b);
            legend.addToggleButton(strongestButton,StringHelper.localize("MINIMAP_LEGEND_DIFFICULTY_STRONGEST"),icon);

            icon = new DOT_SPRITE;
            icon.transform.colorTransform = new ColorTransform(.5, .5, .5, 1, DEFAULT_COLORS[3].r, DEFAULT_COLORS[3].g, DEFAULT_COLORS[3].b);
            legend.addToggleButton(strongButton,StringHelper.localize("MINIMAP_LEGEND_DIFFICULTY_STRONG"),icon);

            icon = new DOT_SPRITE;
            icon.transform.colorTransform = new ColorTransform(.5, .5, .5, 1, DEFAULT_COLORS[2].r, DEFAULT_COLORS[2].g, DEFAULT_COLORS[2].b);
            legend.addToggleButton(normalButton,StringHelper.localize("MINIMAP_LEGEND_DIFFICULTY_NORMAL"),icon);

            icon = new DOT_SPRITE;
            icon.transform.colorTransform = new ColorTransform(.5, .5, .5, 1, DEFAULT_COLORS[1].r, DEFAULT_COLORS[1].g, DEFAULT_COLORS[1].b);
            legend.addToggleButton(weakButton,StringHelper.localize("MINIMAP_LEGEND_DIFFICULTY_WEAK"),icon);

            icon = new DOT_SPRITE;
            icon.transform.colorTransform = new ColorTransform(.5, .5, .5, 1, DEFAULT_COLORS[0].r, DEFAULT_COLORS[0].g, DEFAULT_COLORS[0].b);
            legend.addToggleButton(weakestButton,StringHelper.localize("MINIMAP_LEGEND_DIFFICULTY_WEAKEST"),icon);

        }

        public function addOnChangeListener(callback:Function):void {
            cityButton.addActionListener(callback);
            strongestButton.addActionListener(callback);
            strongButton.addActionListener(callback);
            normalButton.addActionListener(callback);
            weakButton.addActionListener(callback);
            weakestButton.addActionListener(callback);
        }
    }
}