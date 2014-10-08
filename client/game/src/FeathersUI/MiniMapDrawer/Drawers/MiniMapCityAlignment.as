package src.FeathersUI.MiniMapDrawer.Drawers {
    import feathers.controls.ToggleButton;

    import src.FeathersUI.MiniMap.MinimapDotIcon;
    import src.FeathersUI.MiniMapDrawer.LegendGroups.MiniMapGroupCity;

    import src.FeathersUI.MiniMapDrawer.LegendGroups.MiniMapGroupCity;

    import src.Global;
    import src.FeathersUI.MiniMapDrawer.MiniMapLegendPanel;
    import src.FeathersUI.MiniMap.MiniMapRegionObject;
    import src.Objects.Factories.SpriteFactory;
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
            var dotSprite: Image;
            if (Global.map.cities.get(obj.groupId)) {
                // If it's our city, we just show a special flag
                if (cityButton.isSelected) {
                    return;
                }

                dotSprite = SpriteFactory.getStarlingImage("DOT_SPRITE");
                dotSprite.color = MiniMapGroupCity.CITY_DEFAULT_COLOR.hex;
                obj.setIcon(dotSprite);
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

                dotSprite = SpriteFactory.getStarlingImage("DOT_SPRITE");
                dotSprite.color = MiniMapGroupCity.DEFAULT_COLORS[alignmentIdx].hex;
                obj.setIcon(dotSprite);
            }
        }

        public function applyLegend(legend: MiniMapLegendPanel): void {
            var cityIcon: Image = new MinimapDotIcon(true, MiniMapGroupCity.CITY_DEFAULT_COLOR).dot;
            legend.addFilterButton(cityButton, StringHelper.localize("MINIMAP_LEGEND_CITY"), cityIcon);

            var ap100Icon: Image = new MinimapDotIcon(true, MiniMapGroupCity.DEFAULT_COLORS[4]).dot;
            legend.addFilterButton(ap100Button, StringHelper.localize("MINIMAP_LEGEND_ALIGNMENT_90"), ap100Icon);

            var ap80Icon: Image = new MinimapDotIcon(true, MiniMapGroupCity.DEFAULT_COLORS[3]).dot;
            legend.addFilterButton(ap80Button, StringHelper.localize("MINIMAP_LEGEND_ALIGNMENT_75"), ap80Icon);

            var ap60Icon: Image = new MinimapDotIcon(true, MiniMapGroupCity.DEFAULT_COLORS[2]).dot;
            legend.addFilterButton(ap60Button, StringHelper.localize("MINIMAP_LEGEND_ALIGNMENT_50"), ap60Icon);

            var ap40Icon: Image = new MinimapDotIcon(true, MiniMapGroupCity.DEFAULT_COLORS[1]).dot;
            legend.addFilterButton(ap40Button, StringHelper.localize("MINIMAP_LEGEND_ALIGNMENT_25"), ap40Icon);

            var ap20Icon: Image = new MinimapDotIcon(true, MiniMapGroupCity.DEFAULT_COLORS[0]).dot;
            legend.addFilterButton(ap20Button, StringHelper.localize("MINIMAP_LEGEND_ALIGNMENT_10"), ap20Icon);
        }

        public function addOnChangeListener(callback: Function): void {
            cityButton.addEventListener(Event.TRIGGERED, callback);
            ap20Button.addEventListener(Event.TRIGGERED, callback);
            ap40Button.addEventListener(Event.TRIGGERED, callback);
            ap60Button.addEventListener(Event.TRIGGERED, callback);
            ap80Button.addEventListener(Event.TRIGGERED, callback);
            ap100Button.addEventListener(Event.TRIGGERED, callback);
        }
    }
}