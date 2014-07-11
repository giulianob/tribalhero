package src.Map.MiniMap.LegendGroups {
import src.Map.MiniMapDrawers.*;

import flash.events.Event;

import org.aswing.JButton;

import src.Map.MiniMap.MiniMapLegendPanel;
import src.Map.MiniMap.MiniMapRegionObject;
import src.UI.LookAndFeel.GameLookAndFeel;
import src.Util.StringHelper;

public class MiniMapGroupCity {
    private var filter : IMiniMapObjectDrawer = new MiniMapCityDifficulty();
    private var callback : Function;
    private var button: JButton = new JButton(StringHelper.localize("MINIMAP_LEGEND_DIFFICULTY"));
    private var troopDrawer : MiniMapTroopDrawer = new MiniMapTroopDrawer();
    private var legendPanel : MiniMapLegendPanel = new MiniMapLegendPanel();

    public static const CITY_DEFAULT_COLOR: * = { r: 255, g: 255, b: 255, hex: 0xFFFFFF };

    public static const DEFAULT_COLORS: Array = [
        { r: 200, g: 200, b: 200, hex: 0xc8c8c8 },
        { r: 0, g: 156, b: 20, hex: 0x009c14 },
        { r: 255, g: 255, b: 0, hex: 0xffff00 },
        { r: 255, g: 100, b: 0, hex: 0xff6400 },
        { r: 255, g: 0, b: 0, hex: 0xff0000 }
    ];

    public function MiniMapGroupCity() {
        GameLookAndFeel.changeClass(button, "GameJBoxButton");
        button.addActionListener(onFilterChange);

        troopDrawer.addOnChangeListener(onChange);
        filter.addOnChangeListener(onChange);
    }

    private function onChange(e:Event): void {
        if(callback!=null) callback(e);
    }

    private function onFilterChange(e:Event): void {
        if (button.getText() == StringHelper.localize("MINIMAP_LEGEND_DIFFICULTY")) {
            button.setText(StringHelper.localize("MINIMAP_LEGEND_ALIGNMENT"));
            filter = new MiniMapCityAlignment();
        } else if (button.getText() == StringHelper.localize("MINIMAP_LEGEND_ALIGNMENT")) {
            button.setText(StringHelper.localize("MINIMAP_LEGEND_DISTANCE"));
            filter = new MiniMapCityDistance();
        } else if (button.getText() == StringHelper.localize("MINIMAP_LEGEND_DISTANCE")) {
            button.setText(StringHelper.localize("MINIMAP_LEGEND_TRIBE"));
            filter = new MiniMapCityTribe();
        } else if (button.getText() == StringHelper.localize("MINIMAP_LEGEND_TRIBE")) {
            button.setText(StringHelper.localize("MINIMAP_LEGEND_NEWBIE"));
            filter = new MiniMapCityNewbie();
        } else if (button.getText() == StringHelper.localize("MINIMAP_LEGEND_NEWBIE")) {
            button.setText(StringHelper.localize("MINIMAP_LEGEND_DIFFICULTY"));
            filter = new MiniMapCityDifficulty();
        }
        filter.addOnChangeListener(onChange);
        legendPanel.removeAll();
        legendPanel.addRaw(button);
        filter.applyLegend(legendPanel);
        troopDrawer.applyLegend(legendPanel);

        onChange(e);
    }

    public function applyCity(obj:MiniMapRegionObject):void {
        filter.applyObject(obj);
    }

    public function applyTroop(obj:MiniMapRegionObject):void {
        troopDrawer.applyObject(obj);
    }

    public function addOnChangeListener(callback:Function):void {
        this.callback=callback;
    }

    public function getLegendPanel(): MiniMapLegendPanel {
        legendPanel.removeAll();
        legendPanel.addRaw(button);
        filter.applyLegend(legendPanel);
        troopDrawer.applyLegend(legendPanel);
        return legendPanel;
    }


}
}
