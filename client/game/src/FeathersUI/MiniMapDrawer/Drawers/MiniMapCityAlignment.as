package src.FeathersUI.MiniMapDrawer.Drawers {
    import feathers.controls.ToggleButton;

    import src.FeathersUI.MiniMap.MiniMapDiamondIcon;
    import src.FeathersUI.MiniMap.MiniMapRegionObject;
    import src.FeathersUI.MiniMapDrawer.LegendGroups.MiniMapGroupCity;
    import src.FeathersUI.MiniMapDrawer.MiniMapLegendPanel;
    import src.Global;
    import src.Util.StringHelper;

    import starling.display.Image;
    import starling.events.Event;

    public class MiniMapCityAlignment implements IMiniMapObjectDrawer {
        private var cityButton: ToggleButton = new ToggleButton();
        private var ap20Button: ToggleButton = new ToggleButton();
        private var ap40Button: ToggleButton = new ToggleButton();
        private var ap60Button: ToggleButton = new ToggleButton();
        private var ap80Button: ToggleButton = new ToggleButton();
        private var ap100Button: ToggleButton = new ToggleButton();

        public function applyObject(obj: MiniMapRegionObject): void {

            var alignment: Number = obj.extraProps.alignment;
            var alignmentIdx: int;

            if (Global.map.cities.get(obj.groupId)) {
                // If it's our city, we just show a special flag
                if (cityButton.isSelected) {
                    return;
                }

                obj.setIcon(new MiniMapDiamondIcon(MiniMapGroupCity.CITY_DEFAULT_COLOR).dot);
            }
            else {
                if (alignment <= 20 && !ap20Button.isSelected) {
                    alignmentIdx = 0;
                }
                else if (alignment > 20 && alignment <= 40 && !ap40Button.isSelected) {
                    alignmentIdx = 1;
                }
                else if (alignment > 40 && alignment <= 60 && !ap60Button.isSelected) {
                    alignmentIdx = 2;
                }
                else if (alignment > 60 && alignment <= 80 && !ap80Button.isSelected) {
                    alignmentIdx = 3;
                }
                else if (alignment > 80 && !ap100Button.isSelected) {
                    alignmentIdx = 4;
                }
                else {
                    return;
                }

                obj.setIcon(new MiniMapDiamondIcon(MiniMapGroupCity.DEFAULT_COLORS[alignmentIdx]).dot);
            }
        }

        public function applyLegend(legend: MiniMapLegendPanel): void {
            var cityIcon: Image = new MiniMapDiamondIcon(MiniMapGroupCity.CITY_DEFAULT_COLOR).dot;
            legend.addFilterButton(cityButton, StringHelper.localize("MINIMAP_LEGEND_CITY"), cityIcon);

            var ap100Icon: Image = new MiniMapDiamondIcon(MiniMapGroupCity.DEFAULT_COLORS[4]).dot;
            legend.addFilterButton(ap100Button, StringHelper.localize("MINIMAP_LEGEND_ALIGNMENT_90"), ap100Icon);

            var ap80Icon: Image = new MiniMapDiamondIcon(MiniMapGroupCity.DEFAULT_COLORS[3]).dot;
            legend.addFilterButton(ap80Button, StringHelper.localize("MINIMAP_LEGEND_ALIGNMENT_75"), ap80Icon);

            var ap60Icon: Image = new MiniMapDiamondIcon(MiniMapGroupCity.DEFAULT_COLORS[2]).dot;
            legend.addFilterButton(ap60Button, StringHelper.localize("MINIMAP_LEGEND_ALIGNMENT_50"), ap60Icon);

            var ap40Icon: Image = new MiniMapDiamondIcon(MiniMapGroupCity.DEFAULT_COLORS[1]).dot;
            legend.addFilterButton(ap40Button, StringHelper.localize("MINIMAP_LEGEND_ALIGNMENT_25"), ap40Icon);

            var ap20Icon: Image = new MiniMapDiamondIcon(MiniMapGroupCity.DEFAULT_COLORS[0]).dot;
            legend.addFilterButton(ap20Button, StringHelper.localize("MINIMAP_LEGEND_ALIGNMENT_10"), ap20Icon);
        }

        public function addOnChangeListener(callback: Function): void {
            cityButton.addEventListener(Event.CHANGE, callback);
            ap20Button.addEventListener(Event.CHANGE, callback);
            ap40Button.addEventListener(Event.CHANGE, callback);
            ap60Button.addEventListener(Event.CHANGE, callback);
            ap80Button.addEventListener(Event.CHANGE, callback);
            ap100Button.addEventListener(Event.CHANGE, callback);
        }
    }
}